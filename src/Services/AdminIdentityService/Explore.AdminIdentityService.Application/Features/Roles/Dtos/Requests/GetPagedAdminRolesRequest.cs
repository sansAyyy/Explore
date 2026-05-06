using BuildingBlocks.Common.Pagination;
using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;

public sealed class GetPagedAdminRolesRequest : PagedRequest
{
    [MaxLength(128)]
    public string? Keyword { get; set; }

    public bool? IsActive { get; set; }
}

