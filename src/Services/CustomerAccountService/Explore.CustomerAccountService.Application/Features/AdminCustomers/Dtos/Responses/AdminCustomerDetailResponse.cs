namespace Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;

public sealed record AdminCustomerDetailResponse(
    Guid Id,
    string PhoneNumber,
    string NickName,
    string? AvatarUrl,
    string? Email,
    bool IsActive,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    DateTime? LastLoginAt,
    long Version);

