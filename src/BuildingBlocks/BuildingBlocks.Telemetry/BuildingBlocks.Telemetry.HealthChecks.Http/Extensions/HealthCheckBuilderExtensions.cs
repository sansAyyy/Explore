using BuildingBlocks.Telemetry.HealthChecks.AspNetCore;
using BuildingBlocks.Telemetry.HealthChecks.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.Http.Extensions;

public static class HealthCheckBuilderExtensions
{
    private static readonly string[] ReadyTags = [HealthCheckTags.Ready];

    public static IHealthChecksBuilder AddHttpDependencyReadiness(
        this IHealthChecksBuilder builder,
        string? baseAddress,
        string name,
        string healthPath = "/health/ready",
        TimeSpan? timeout = null)
    {
        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var baseAddressUri))
        {
            throw new InvalidOperationException($"HTTP dependency '{name}' BaseUrl must be an absolute URI.");
        }

        builder.Add(new HealthCheckRegistration(
            name,
            _ => new HttpDependencyHealthCheck(baseAddressUri, healthPath, timeout ?? TimeSpan.FromSeconds(5)),
            HealthStatus.Unhealthy,
            ReadyTags,
            timeout ?? TimeSpan.FromSeconds(5)));

        return builder;
    }

    public static IHealthChecksBuilder AddReverseProxyClusterReadiness(
        this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        var clusters = configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        foreach (var cluster in clusters)
        {
            foreach (var destination in cluster.GetSection("Destinations").GetChildren())
            {
                var address = destination["Address"];
                var name = $"reverse-proxy:{cluster.Key}:{destination.Key}";
                builder.AddHttpDependencyReadiness(address, name);
            }
        }

        return builder;
    }
}
