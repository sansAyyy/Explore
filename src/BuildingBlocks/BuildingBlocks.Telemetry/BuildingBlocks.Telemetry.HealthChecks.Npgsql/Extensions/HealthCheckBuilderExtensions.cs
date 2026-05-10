using BuildingBlocks.Telemetry.HealthChecks.AspNetCore;
using BuildingBlocks.Telemetry.HealthChecks.Npgsql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.Npgsql.Extensions;

public static class HealthCheckBuilderExtensions
{
    private static readonly string[] ReadyTags = [HealthCheckTags.Ready];

    public static IHealthChecksBuilder AddPostgreSqlReadiness(
        this IHealthChecksBuilder builder,
        string? connectionString,
        string name = "postgresql")
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string for readiness check '{name}' is required.");
        }

        builder.Add(new HealthCheckRegistration(
            name,
            _ => new NpgsqlHealthCheck(connectionString),
            HealthStatus.Unhealthy,
            ReadyTags,
            TimeSpan.FromSeconds(5)));

        return builder;
    }
}
