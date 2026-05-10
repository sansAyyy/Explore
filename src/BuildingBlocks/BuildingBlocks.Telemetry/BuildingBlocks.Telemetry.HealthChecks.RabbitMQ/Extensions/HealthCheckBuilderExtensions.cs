using BuildingBlocks.Telemetry.HealthChecks.AspNetCore;
using BuildingBlocks.Telemetry.HealthChecks.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.RabbitMQ.Extensions;

public static class HealthCheckBuilderExtensions
{
    private static readonly string[] ReadyTags = [HealthCheckTags.Ready];

    public static IHealthChecksBuilder AddRabbitMqReadiness(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string name = "rabbitmq")
    {
        var options = configuration
            .GetSection("RabbitMqOptions")
            .Get<RabbitMqHealthCheckOptions>() ?? new RabbitMqHealthCheckOptions();

        builder.Add(new HealthCheckRegistration(
            name,
            _ => new RabbitMqHealthCheck(options),
            HealthStatus.Unhealthy,
            ReadyTags,
            TimeSpan.FromSeconds(5)));

        return builder;
    }
}
