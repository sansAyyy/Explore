namespace Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Requests;

public sealed class AssignUserRolesRequest
{
    public IReadOnlyCollection<Guid> RoleIds { get; set; } = [];
}

