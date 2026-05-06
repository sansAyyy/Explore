using BuildingBlocks.Correlation.Abstractions.Constants;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Messaging.RabbitMQ.Resolvers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.RabbitMQ.Filters
{
    public class CorrelationConsumeFilter<T> : IFilter<ConsumeContext<T>>
        where T : class
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly ILogger<CorrelationConsumeFilter<T>> _logger;

        public CorrelationConsumeFilter(
            ICorrelationContextAccessor correlationContextAccessor,
            ILogger<CorrelationConsumeFilter<T>> logger)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _logger = logger;
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var correlationId = CorrelationIdResolver.Resolve(context);
            _correlationContextAccessor.CorrelationId = correlationId;

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                [CorrelationIdConstants.LogPropertyName] = correlationId
            }))
            {
                _logger.LogInformation(
                    "Consuming message {MessageType} with CorrelationId {CorrelationId}",
                    typeof(T).Name,
                    correlationId);

                await next.Send(context);
            }
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}

