using BuildingBlocks.Common.Pagination;
using Explore.AdminIdentityService.Domain.AdminPermissions;
using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;

public sealed class GetPagedAdminPermissionsRequest : PagedRequest
{
    [MaxLength(128)]
    public string? Keyword { get; set; }

    public bool? IsActive { get; set; }

    public PermissionResourceType? ResourceType { get; set; }
}

