namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Responses;

public sealed record AdminUserBasicResponse(
    Guid Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt);

