namespace BuildingBlocks.Messaging.Outbox.Abstractions;

public interface IOutboxMessageWriter
{
    Task WriteAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;
}

