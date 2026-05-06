using Explore.AdminIdentityService.Domain.AdminPermissions;

namespace Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Responses;

public sealed record AdminPermissionDetailResponse(
    Guid Id,
    Guid? ParentId,
    string Code,
    string Name,
    string? Description,
    PermissionResourceType ResourceType,
    bool IsActive,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    long Version);

