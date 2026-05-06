using BuildingBlocks.Messaging.Abstractions.Envelope;

namespace BuildingBlocks.Messaging.Inbox.Abstractions;

public interface IInboxMessageProcessor
{
    Task<bool> ProcessAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        string consumerName,
        Func<CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}

