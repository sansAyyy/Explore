using BuildingBlocks.Common.Http;
using BuildingBlocks.CurrentUser.AspNetCore.Extensions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.RabbitMQ.Extensions;
using BuildingBlocks.Persistence.Auditing.CurrentUser.Extensions;
using BuildingBlocks.Security.Authentication.Jwt.Extensions;
using Explore.MessageCenterService.Application.Abstractions.External;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Infrastructure.External;
using Explore.MessageCenterService.Infrastructure.Messaging.Consumers;
using Explore.MessageCenterService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.MessageCenterService.Infrastructure;

public class DependencyInjection : IModuleDependencyRegistrar
{
    public IServiceCollection ModuleDependencyInjection(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MessageCenterDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'MessageCenterDatabase' is required.");
        }

        var recipientDirectoryOptions = configuration.GetSection(RecipientDirectoryOptions.SectionName).Get<RecipientDirectoryOptions>()
            ?? new RecipientDirectoryOptions();
        services.Configure<RecipientDirectoryOptions>(configuration.GetSection(RecipientDirectoryOptions.SectionName));

        services.AddDbContext<MessageCenterDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(MessageCenterDbContext).Assembly.FullName)));

        services.AddAspNetCoreCurrentUser();
        services.AddCurrentUserAuditActorAccessor();

        services.AddRabbitMQ(configuration, typeof(SendNotificationByTemplateMessageConsumer).Assembly);

        services.AddEntityFrameworkInbox<MessageCenterDbContext>();

        services.AddJwtAuthentication(configuration);
        services.AddScoped<IMessageCenterUnitOfWork, MessageCenterUnitOfWork>();

        services.AddTypedHttpClient<IRecipientDirectoryClient, HttpRecipientDirectoryClient>(
            recipientDirectoryOptions.BaseUrl.ToRequiredAbsoluteUri("RecipientDirectoryOptions.BaseUrl"),
            recipientDirectoryOptions.Timeout,
            OutboundHttpResilienceProfile.ReadOnly);

        return services;
    }
}



