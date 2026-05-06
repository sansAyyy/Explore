namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Options;

public sealed class OutboxDispatcherOptions
{
    public int BatchSize { get; set; } = 50;

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

