namespace Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;

public sealed record AdminRoleDetailResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    long Version);

