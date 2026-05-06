namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Responses;

public sealed record AdminUserDetailResponse(
    Guid Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    DateTime? LastLoginAt,
    long Version);

