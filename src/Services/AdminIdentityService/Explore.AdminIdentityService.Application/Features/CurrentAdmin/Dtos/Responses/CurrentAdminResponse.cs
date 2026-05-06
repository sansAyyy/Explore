namespace Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Responses;

public sealed record CurrentAdminResponse(
    Guid Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    long Version);

