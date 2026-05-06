using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Caching.Redis.Extensions;
using BuildingBlocks.Caching.Redis.Services;
using BuildingBlocks.DistributedLocking.Abstractions;
using BuildingBlocks.DistributedLocking.Redis.Extensions;
using BuildingBlocks.DistributedLocking.Redis.Services;
using FreeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class RedisRegistrationTests
{
    [Fact]
    public void AddRedisCaching_ShouldRegisterCacheService()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddRedisCaching(configuration);

        using var provider = services.BuildServiceProvider();
        var cacheService = provider.GetRequiredService<ICacheService>();
        var redisClient = provider.GetRequiredService<RedisClient>();

        Assert.IsType<RedisCacheService>(cacheService);
        Assert.NotNull(redisClient);
    }

    [Fact]
    public void AddRedisDistributedLocking_ShouldRegisterDistributedLockService()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddRedisDistributedLocking(configuration);

        using var provider = services.BuildServiceProvider();
        var distributedLockService = provider.GetRequiredService<IDistributedLockService>();
        var redisClient = provider.GetRequiredService<RedisClient>();

        Assert.IsType<RedisDistributedLockService>(distributedLockService);
        Assert.NotNull(redisClient);
    }

    [Fact]
    public void AddRedisCachingAndAddRedisDistributedLocking_ShouldReuseSingleRedisClientRegistration()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddRedisCaching(configuration);
        services.AddRedisDistributedLocking(configuration);

        using var provider = services.BuildServiceProvider();
        var redisClients = provider.GetServices<RedisClient>().ToArray();

        Assert.Single(redisClients);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = "localhost:6379,defaultDatabase=0"
            })
            .Build();
    }
}

