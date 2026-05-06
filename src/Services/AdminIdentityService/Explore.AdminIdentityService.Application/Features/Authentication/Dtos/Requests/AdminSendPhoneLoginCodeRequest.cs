using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;

public sealed class AdminSendPhoneLoginCodeRequest
{
    [Required]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;
}

