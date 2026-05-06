using BuildingBlocks.Common.Pagination;
using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;

public sealed class GetPagedAdminUsersRequest : PagedRequest
{
    [MaxLength(128)]
    public string? Keyword { get; set; }

    public bool? IsActive { get; set; }
}

