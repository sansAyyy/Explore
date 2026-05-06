using System.ComponentModel.DataAnnotations;

namespace Explore.CustomerAccountService.Application.Features.CurrentCustomer;

public interface ICurrentCustomerAppService
{
    Task<BuildingBlocks.Common.Results.Result<CurrentCustomerResponse>> GetCurrentAsync(CancellationToken cancellationToken);

    Task<BuildingBlocks.Common.Results.Result<CurrentCustomerResponse>> UpdateProfileAsync(
        UpdateCurrentCustomerProfileRequest request,
        CancellationToken cancellationToken);

    Task<BuildingBlocks.Common.Results.Result<CurrentCustomerResponse>> UpdateAvatarAsync(
        UpdateCurrentCustomerAvatarRequest request,
        CancellationToken cancellationToken);
}

public sealed class UpdateCurrentCustomerProfileRequest
{
    [Required]
    [MaxLength(64)]
    public string NickName { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? AvatarUrl { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }
}

public sealed class UpdateCurrentCustomerAvatarRequest
{
    [MaxLength(512)]
    public string? AvatarUrl { get; set; }
}

public sealed record CurrentCustomerResponse(
    Guid Id,
    string PhoneNumber,
    string NickName,
    string? AvatarUrl,
    string? Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    long Version);

