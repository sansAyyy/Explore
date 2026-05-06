namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Models;

public sealed class OutboxMessage
{
    public Guid MessageId { get; set; }

    public string PayloadType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime OccurredOn { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }

    public DateTime? LockedUntil { get; set; }
}

