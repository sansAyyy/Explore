using Explore.AdminIdentityService.Domain.AdminPermissions;
using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;

public sealed class UpdateAdminPermissionRequest
{
    public Guid? ParentId { get; set; }

    [Required]
    [MaxLength(128)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Description { get; set; }

    public PermissionResourceType ResourceType { get; set; }
}

