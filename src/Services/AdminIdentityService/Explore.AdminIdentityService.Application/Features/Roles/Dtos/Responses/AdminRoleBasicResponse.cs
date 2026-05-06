namespace Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;

public sealed record AdminRoleBasicResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

