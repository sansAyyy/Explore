using Explore.AdminIdentityService.Domain.AdminPermissions;

namespace Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;

public sealed record AssignedRolePermissionResponse(
    Guid Id,
    string Code,
    string Name,
    PermissionResourceType ResourceType,
    bool IsActive);

