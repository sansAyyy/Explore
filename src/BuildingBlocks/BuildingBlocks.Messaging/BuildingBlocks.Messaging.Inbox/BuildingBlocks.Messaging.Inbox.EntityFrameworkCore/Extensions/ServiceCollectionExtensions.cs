using BuildingBlocks.Messaging.Inbox.Abstractions;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Processors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkInbox<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IInboxMessageProcessor, EntityFrameworkInboxMessageProcessor<TDbContext>>();
        return services;
    }
}

