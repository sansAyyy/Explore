namespace Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;

public sealed class AssignRolePermissionsRequest
{
    public IReadOnlyCollection<Guid> PermissionIds { get; set; } = [];
}

