using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Requests;

public sealed class ChangeCurrentAdminPasswordRequest
{
    [Required]
    [MaxLength(128)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}

