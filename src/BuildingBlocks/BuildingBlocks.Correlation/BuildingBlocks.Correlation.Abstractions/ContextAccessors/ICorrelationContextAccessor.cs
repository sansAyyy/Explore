namespace BuildingBlocks.Correlation.Abstractions.ContextAccessors
{
    public interface ICorrelationContextAccessor
    {
        string? CorrelationId { get; set; }
    }
}

