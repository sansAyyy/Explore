using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.Persistence.Auditing.Abstractions;

namespace BuildingBlocks.Persistence.Auditing.CurrentUser.Services;

public sealed class CurrentUserAuditActorAccessor : IAuditActorAccessor
{
    private readonly ICurrentUser _currentUser;

    public CurrentUserAuditActorAccessor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public string AuditActor => _currentUser.AuditActor;
}
