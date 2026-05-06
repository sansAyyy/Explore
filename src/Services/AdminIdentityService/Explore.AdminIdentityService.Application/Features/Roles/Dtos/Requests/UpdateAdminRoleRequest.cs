using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;

public sealed class UpdateAdminRoleRequest
{
    [Required]
    [MaxLength(128)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Description { get; set; }
}

