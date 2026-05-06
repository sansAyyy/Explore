using BuildingBlocks.Common.Http;
using BuildingBlocks.Caching.Redis.Extensions;
using BuildingBlocks.CurrentUser.AspNetCore.Extensions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.DistributedLocking.Redis.Extensions;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.RabbitMQ.Extensions;
using BuildingBlocks.Persistence.Auditing.CurrentUser.Extensions;
using BuildingBlocks.Security.Authentication.Jwt.Extensions;
using BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Extensions;
using BuildingBlocks.Security.PhoneVerification.Extensions;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Infrastructure.MessageCenter;
using Explore.AdminIdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.AdminIdentityService.Infrastructure;

public class DependencyInjection : IModuleDependencyRegistrar
{
    public IServiceCollection ModuleDependencyInjection(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AdminIdentityDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'AdminIdentityDatabase' is required.");
        }

        var messageCenterOptions = configuration.GetSection(MessageCenterClientOptions.SectionName).Get<MessageCenterClientOptions>()
            ?? new MessageCenterClientOptions();
        services.Configure<MessageCenterClientOptions>(configuration.GetSection(MessageCenterClientOptions.SectionName));

        services.AddDbContext<AdminIdentityDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(AdminIdentityDbContext).Assembly.FullName)));

        services.AddAspNetCoreCurrentUser();
        services.AddCurrentUserAuditActorAccessor();

        services.AddJwtTokenService(configuration);
        services.AddJwtAuthentication(configuration);

        services.AddRedisCaching(configuration);
        services.AddRedisDistributedLocking(configuration);
        services.AddPhoneVerificationCode(configuration);
        services.AddAspNetCoreIdentityPasswordHashService();

        services.AddRabbitMQ(configuration, typeof(DependencyInjection).Assembly);

        services.AddEntityFrameworkOutbox<AdminIdentityDbContext>();

        services.AddScoped<IAdminIdentityUnitOfWork, AdminIdentityUnitOfWork>();
        services.AddTypedHttpClient<IAdminMessageCenterClient, MessageCenterClient>(
            messageCenterOptions.BaseUrl.ToRequiredAbsoluteUri("MessageCenter.BaseUrl"),
            messageCenterOptions.Timeout,
            OutboundHttpResilienceProfile.CommandNoRetry);

        return services;
    }
}


