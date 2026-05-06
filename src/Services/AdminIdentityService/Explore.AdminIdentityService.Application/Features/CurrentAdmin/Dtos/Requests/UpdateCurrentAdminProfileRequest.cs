using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Requests;

public sealed class UpdateCurrentAdminProfileRequest
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
}

