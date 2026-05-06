namespace BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Models;

public sealed class InboxMessage
{
    public Guid MessageId { get; set; }

    public string ConsumerName { get; set; } = string.Empty;

    public string PayloadType { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }

    public DateTime ProcessedAt { get; set; }
}

