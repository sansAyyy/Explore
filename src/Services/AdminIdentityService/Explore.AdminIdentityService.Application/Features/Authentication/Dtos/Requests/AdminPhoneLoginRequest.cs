using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;

public sealed class AdminPhoneLoginRequest
{
    [Required]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string VerificationCode { get; set; } = string.Empty;
}

