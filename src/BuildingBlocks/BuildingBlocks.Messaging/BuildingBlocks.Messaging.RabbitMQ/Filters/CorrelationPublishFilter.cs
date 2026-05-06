using BuildingBlocks.Correlation.Abstractions.Constants;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Messaging.RabbitMQ.Resolvers;
using MassTransit;

namespace BuildingBlocks.Messaging.RabbitMQ.Filters
{
    public class CorrelationPublishFilter<T> : IFilter<PublishContext<T>>
        where T : class
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationPublishFilter(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            var correlationId = CorrelationIdResolver.Resolve(context, _correlationContextAccessor.CorrelationId);
            _correlationContextAccessor.CorrelationId = correlationId;

            if (!context.CorrelationId.HasValue && Guid.TryParse(correlationId, out var correlationGuid))
            {
                context.CorrelationId = correlationGuid;
            }

            context.Headers.Set(CorrelationIdConstants.HeaderName, correlationId);

            await next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}

