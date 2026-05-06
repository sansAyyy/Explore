using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;

public sealed class ChangeAdminUserPasswordRequest
{
    [Required]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}

