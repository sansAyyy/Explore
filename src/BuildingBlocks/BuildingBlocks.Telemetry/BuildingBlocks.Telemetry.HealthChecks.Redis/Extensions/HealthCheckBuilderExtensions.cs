using BuildingBlocks.Telemetry.HealthChecks.AspNetCore;
using BuildingBlocks.Telemetry.HealthChecks.Redis;
using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.Redis.Extensions;

public static class HealthCheckBuilderExtensions
{
    private static readonly string[] ReadyTags = [HealthCheckTags.Ready];

    public static IHealthChecksBuilder AddRedisReadiness(
        this IHealthChecksBuilder builder,
        string? connectionString,
        string name = "redis")
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string for readiness check '{name}' is required.");
        }

        builder.Services.TryAddSingleton(_ => new RedisClient(connectionString));
        builder.Add(new HealthCheckRegistration(
            name,
            provider => new RedisHealthCheck(provider.GetRequiredService<RedisClient>()),
            HealthStatus.Unhealthy,
            ReadyTags,
            TimeSpan.FromSeconds(5)));

        return builder;
    }
}
