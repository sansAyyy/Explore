namespace BuildingBlocks.Telemetry.HealthChecks.RabbitMQ;

internal sealed class RabbitMqHealthCheckOptions
{
    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";
}
