namespace BuildingBlocks.Domain.Abstractions;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    string CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    string? UpdatedBy { get; }
    bool IsDeleted { get; }
    long Version { get; }
}
