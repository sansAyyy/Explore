using BuildingBlocks.Messaging.Outbox.Abstractions;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Dispatchers;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Options;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Writers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkOutbox<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IOutboxMessageWriter, EntityFrameworkOutboxMessageWriter<TDbContext>>();
        services.AddScoped<EntityFrameworkOutboxDispatcher<TDbContext>>();
        services.AddHostedService<OutboxDispatcherHostedService<TDbContext>>();
        services.AddOptions<OutboxDispatcherOptions>();
        return services;
    }
}

