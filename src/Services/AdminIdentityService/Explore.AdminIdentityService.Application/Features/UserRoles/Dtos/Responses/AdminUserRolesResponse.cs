namespace Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Responses;

public sealed record AdminUserRolesResponse(
    Guid AdminUserId,
    IReadOnlyCollection<AssignedUserRoleResponse> Roles);

