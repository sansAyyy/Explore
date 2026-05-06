using BuildingBlocks.Caching.Abstractions;
using FreeRedis;

namespace BuildingBlocks.Caching.Redis.Services;

public class RedisCacheService : ICacheService
{
    private readonly RedisClient _redisClient;

    public RedisCacheService(RedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return _redisClient.ExistsAsync(key);
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        return _redisClient.GetAsync<T?>(key);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        return _redisClient.DelAsync(key);
    }

    public Task SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        return _redisClient.SetAsync(key, value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        return _redisClient.SetAsync(key, value, ttl);
    }
}

