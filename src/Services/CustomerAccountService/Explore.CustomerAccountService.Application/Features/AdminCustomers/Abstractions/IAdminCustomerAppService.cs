using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;

namespace Explore.CustomerAccountService.Application.Features.AdminCustomers.Abstractions;

public interface IAdminCustomerAppService
{
    Task<Result<PagedResult<AdminCustomerBasicResponse>>> GetPagedAsync(
        GetPagedAdminCustomersRequest request,
        CancellationToken cancellationToken);

    Task<Result<AdminCustomerDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken);
}

