using BuildingBlocks.Telemetry.HealthChecks.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Telemetry.HealthChecks.AspNetCore.Extensions;

public static class HealthCheckBuilderExtensions
{
    private static readonly string[] LiveTags = [HealthCheckTags.Live];

    public static IHealthChecksBuilder AddServiceHealthChecks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: LiveTags);
    }
}
