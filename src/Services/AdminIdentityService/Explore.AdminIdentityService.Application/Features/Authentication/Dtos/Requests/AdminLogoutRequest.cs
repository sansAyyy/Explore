using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;

public sealed class AdminLogoutRequest
{
    [Required]
    [MaxLength(1024)]
    public string RefreshToken { get; set; } = string.Empty;
}

