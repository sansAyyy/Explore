namespace Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;

public sealed record AdminRolePermissionsResponse(
    Guid RoleId,
    IReadOnlyCollection<AssignedRolePermissionResponse> Permissions);

