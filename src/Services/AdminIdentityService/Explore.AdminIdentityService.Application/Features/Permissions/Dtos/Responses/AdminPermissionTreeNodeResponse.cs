using Explore.AdminIdentityService.Domain.AdminPermissions;

namespace Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Responses;

public sealed record AdminPermissionTreeNodeResponse(
    Guid Id,
    Guid? ParentId,
    string Code,
    string Name,
    string? Description,
    PermissionResourceType ResourceType,
    bool IsActive,
    IReadOnlyCollection<AdminPermissionTreeNodeResponse> Children);

