using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Messaging.Abstractions.Abstractions;
using MassTransit;
using BuildingBlocks.Messaging.Abstractions.Envelope;

namespace BuildingBlocks.Messaging.RabbitMQ.Publishers
{
    public class MassTransitEventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public MassTransitEventPublisher(
            IPublishEndpoint publishEndpoint,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            _publishEndpoint = publishEndpoint;
            _correlationContextAccessor = correlationContextAccessor;
        }

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            ArgumentNullException.ThrowIfNull(@event);

            var envelope = MessageEnvelope<TEvent>.Create(
                @event,
                Guid.NewGuid(),
                DateTime.UtcNow,
                _correlationContextAccessor.CorrelationId);

            return _publishEndpoint.Publish(envelope, cancellationToken);
        }
    }
}

