using System.Text.Json;
using BuildingBlocks.Telemetry.HealthChecks.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.AspNetCore.Extensions;

public static class EndpointRouteBuilderExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapServiceHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live),
            ResponseWriter = WriteHealthCheckResponseAsync
        });

        var readinessOptions = new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(HealthCheckTags.Ready),
            ResponseWriter = WriteHealthCheckResponseAsync
        };

        endpoints.MapHealthChecks("/health/ready", readinessOptions);
        endpoints.MapHealthChecks("/health", readinessOptions);

        return endpoints;
    }

    private static Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration,
            entries = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration
                })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonSerializerOptions));
    }
}
