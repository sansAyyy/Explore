namespace BuildingBlocks.Persistence.Auditing.Abstractions;

public interface IAuditActorAccessor
{
    string AuditActor { get; }
}
