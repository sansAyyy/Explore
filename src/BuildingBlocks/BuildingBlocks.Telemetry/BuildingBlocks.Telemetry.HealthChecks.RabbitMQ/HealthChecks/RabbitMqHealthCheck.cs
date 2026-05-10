using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace BuildingBlocks.Telemetry.HealthChecks.RabbitMQ;

internal sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqHealthCheckOptions _options;

    public RabbitMqHealthCheck(RabbitMqHealthCheckOptions options)
    {
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ readiness check failed.", exception);
        }
    }
}
