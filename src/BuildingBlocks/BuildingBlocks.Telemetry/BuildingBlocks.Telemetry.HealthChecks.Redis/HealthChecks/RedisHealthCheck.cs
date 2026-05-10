using FreeRedis;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.Redis;

internal sealed class RedisHealthCheck : IHealthCheck
{
    private readonly RedisClient _redisClient;

    public RedisHealthCheck(RedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() => _redisClient.Ping("health"), cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis readiness check failed.", exception);
        }
    }
}
