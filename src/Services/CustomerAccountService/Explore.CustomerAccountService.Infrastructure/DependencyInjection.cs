using BuildingBlocks.Common.Http;
using BuildingBlocks.Caching.Redis.Extensions;
using BuildingBlocks.CurrentUser.AspNetCore.Extensions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.DistributedLocking.Redis.Extensions;
using BuildingBlocks.Persistence.Auditing.CurrentUser.Extensions;
using BuildingBlocks.Security.Authentication.Jwt.Extensions;
using BuildingBlocks.Security.PhoneVerification.Extensions;
using Explore.CustomerAccountService.Application.Abstractions.Notifications;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;
using Explore.CustomerAccountService.Infrastructure.MessageCenter;
using Explore.CustomerAccountService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.CustomerAccountService.Infrastructure;

public class DependencyInjection : IModuleDependencyRegistrar
{
    public IServiceCollection ModuleDependencyInjection(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CustomerAccountDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'CustomerAccountDatabase' is required.");
        }

        var messageCenterOptions = configuration.GetSection(MessageCenterClientOptions.SectionName).Get<MessageCenterClientOptions>()
            ?? new MessageCenterClientOptions();
        services.Configure<MessageCenterClientOptions>(configuration.GetSection(MessageCenterClientOptions.SectionName));

        services.AddDbContext<CustomerAccountDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(CustomerAccountDbContext).Assembly.FullName)));

        services.AddAspNetCoreCurrentUser();
        services.AddCurrentUserAuditActorAccessor();
        services.AddRedisCaching(configuration);
        services.AddRedisDistributedLocking(configuration);
        services.AddPhoneVerificationCode(configuration);
        services.AddJwtTokenService(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddScoped<ICustomerAccountUnitOfWork, CustomerAccountUnitOfWork>();
        services.AddTypedHttpClient<IMessageCenterClient, MessageCenterClient>(
            messageCenterOptions.BaseUrl.ToRequiredAbsoluteUri("MessageCenter.BaseUrl"),
            messageCenterOptions.Timeout,
            OutboundHttpResilienceProfile.CommandNoRetry);

        return services;
    }
}

