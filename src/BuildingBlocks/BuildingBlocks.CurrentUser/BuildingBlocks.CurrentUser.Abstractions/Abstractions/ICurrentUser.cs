namespace BuildingBlocks.CurrentUser.Abstractions
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string AuditActor { get; }
    }
}

