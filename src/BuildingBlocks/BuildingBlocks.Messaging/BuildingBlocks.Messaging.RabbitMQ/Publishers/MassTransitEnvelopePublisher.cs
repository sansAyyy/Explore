using BuildingBlocks.Messaging.Abstractions.Abstractions;
using MassTransit;

namespace BuildingBlocks.Messaging.RabbitMQ.Publishers;

public sealed class MassTransitEnvelopePublisher : IEnvelopePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEnvelopePublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync(object envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        return _publishEndpoint.Publish(envelope, cancellationToken);
    }
}

