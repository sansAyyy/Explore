using BuildingBlocks.Common.Pagination;
using System.ComponentModel.DataAnnotations;

namespace Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;

public sealed class GetPagedAdminCustomersRequest : PagedRequest
{
    [MaxLength(128)]
    public string? Keyword { get; set; }

    public bool? IsActive { get; set; }
}

