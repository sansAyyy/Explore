using System.Text.Json;
using BuildingBlocks.DistributedLocking.Abstractions;
using BuildingBlocks.DistributedLocking.Redis.Internal;
using BuildingBlocks.DistributedLocking.Redis.Services;
using FreeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.DistributedLocking.Redis.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisDistributedLocking(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");
        ArgumentNullException.ThrowIfNull(connectionString, "Connection string 'Redis' is required.");

        services.TryAddSingleton(_ =>
        {
            var client = new RedisClient(connectionString);

            client.Serialize = obj => JsonSerializer.Serialize(obj);
            client.Deserialize = (json, type) => JsonSerializer.Deserialize(json, type);

#if DEBUG
            client.Notice += (_, e) => Console.WriteLine($"FreeRedis: {e.Log}");
#endif
            return client;
        });

        services.AddScoped<IRedisDistributedLockStore, RedisDistributedLockStore>();
        services.AddScoped<IDistributedLockService>(provider =>
            new RedisDistributedLockService(provider.GetRequiredService<IRedisDistributedLockStore>()));
        return services;
    }
}

