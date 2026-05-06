using BuildingBlocks.Correlation.Abstractions.ContextAccessors;

namespace BuildingBlocks.Messaging.RabbitMQ.ContextAccessors
{
    internal sealed class NullCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public string? CorrelationId { get; set; }
    }
}

