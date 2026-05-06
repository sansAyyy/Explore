using BuildingBlocks.Correlation.Abstractions.ContextAccessors;

namespace BuildingBlocks.Correlation.AspNetCore.ContextAccessors
{
    public class CorrelationContextAccessor : ICorrelationContextAccessor
    {
        public string? CorrelationId { get; set; }
    }
}

