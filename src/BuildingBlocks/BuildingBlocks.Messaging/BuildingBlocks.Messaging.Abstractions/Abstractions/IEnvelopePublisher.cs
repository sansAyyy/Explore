namespace BuildingBlocks.Messaging.Abstractions.Abstractions;

public interface IEnvelopePublisher
{
    Task PublishAsync(object envelope, CancellationToken cancellationToken = default);
}

