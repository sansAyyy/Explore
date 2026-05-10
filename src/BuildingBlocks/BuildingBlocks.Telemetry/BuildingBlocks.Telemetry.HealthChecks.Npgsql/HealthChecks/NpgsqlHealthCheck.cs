using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace BuildingBlocks.Telemetry.HealthChecks.Npgsql;

internal sealed class NpgsqlHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public NpgsqlHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL readiness check failed.", exception);
        }
    }
}
