namespace Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Responses;

public sealed record AssignedUserRoleResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsActive);

