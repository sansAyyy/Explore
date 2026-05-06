namespace Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;

public sealed record AdminCustomerBasicResponse(
    Guid Id,
    string PhoneNumber,
    string NickName,
    string? AvatarUrl,
    string? Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt);

