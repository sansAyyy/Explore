using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;

public sealed class CreateAdminUserRequest
{
    [Required]
    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

