using BuildingBlocks.Telemetry.OpenTelemetry.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Telemetry.OpenTelemetry.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name is required.", nameof(serviceName));
        }

        var telemetryOptions = configuration
            .GetSection(TelemetryOptions.SectionName)
            .Get<TelemetryOptions>() ?? new TelemetryOptions();

        services.Configure<TelemetryOptions>(configuration.GetSection(TelemetryOptions.SectionName));

        if (!telemetryOptions.Enabled)
        {
            return services;
        }

        if (!Uri.TryCreate(telemetryOptions.OtlpEndpoint, UriKind.Absolute, out var otlpEndpoint))
        {
            throw new InvalidOperationException("TelemetryOptions.OtlpEndpoint must be an absolute URI.");
        }

        services
            .AddOpenTelemetry()
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder
                    .AddService(serviceName)
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("deployment.environment", telemetryOptions.Environment)
                    });
            })
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .AddSource("BuildingBlocks.Messaging.RabbitMQ")
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation(options => options.RecordException = true)
                    .AddNpgsql()
                    .AddOtlpExporter(options => options.Endpoint = otlpEndpoint);
            })
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddNpgsqlInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("System.Net.Http")
                    .AddMeter("Npgsql")
                    .AddMeter("BuildingBlocks.Messaging.Inbox")
                    .AddMeter("BuildingBlocks.Messaging.Outbox")
                    .AddOtlpExporter(options => options.Endpoint = otlpEndpoint);
            });

        return services;
    }
}
