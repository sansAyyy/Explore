using System.Text.Json;
using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Caching.Redis.Services;
using FreeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Caching.Redis.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
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

        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }
}

