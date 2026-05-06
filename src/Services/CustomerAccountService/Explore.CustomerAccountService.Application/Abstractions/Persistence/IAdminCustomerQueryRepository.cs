using BuildingBlocks.Common.Pagination;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;

namespace Explore.CustomerAccountService.Application.Abstractions.Persistence;

public interface IAdminCustomerQueryRepository
{
    Task<AdminCustomerDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<AdminCustomerBasicResponse>> GetPagedAsync(
        GetPagedAdminCustomersRequest request,
        CancellationToken cancellationToken);
}

