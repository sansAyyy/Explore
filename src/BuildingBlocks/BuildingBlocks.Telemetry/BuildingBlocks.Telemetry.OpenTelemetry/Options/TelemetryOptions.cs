namespace BuildingBlocks.Telemetry.OpenTelemetry.Options;

public sealed class TelemetryOptions
{
    public const string SectionName = "TelemetryOptions";

    public bool Enabled { get; init; } = true;

    public string Environment { get; init; } = "Development";

    public string OtlpEndpoint { get; init; } = "http://localhost:4317";
}
