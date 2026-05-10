using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.Http;

internal sealed class HttpDependencyHealthCheck : IHealthCheck
{
    private readonly Uri _baseAddress;
    private readonly string _healthPath;
    private readonly TimeSpan _timeout;

    public HttpDependencyHealthCheck(Uri baseAddress, string healthPath, TimeSpan timeout)
    {
        _baseAddress = baseAddress;
        _healthPath = healthPath;
        _timeout = timeout;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(_timeout);

            using var httpClient = new HttpClient
            {
                BaseAddress = _baseAddress,
                Timeout = Timeout.InfiniteTimeSpan
            };

            using var response = await httpClient.GetAsync(_healthPath, timeoutSource.Token);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy();
            }

            return HealthCheckResult.Unhealthy(
                $"HTTP dependency readiness check returned {(int)response.StatusCode}.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("HTTP dependency readiness check failed.", exception);
        }
    }
}
