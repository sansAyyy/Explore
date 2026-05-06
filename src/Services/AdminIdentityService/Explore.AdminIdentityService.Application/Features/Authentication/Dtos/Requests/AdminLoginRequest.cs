using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;

public sealed class AdminLoginRequest
{
    [Required]
    [MaxLength(256)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

