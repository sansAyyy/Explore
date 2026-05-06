using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Abstractions;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Validators;

namespace Explore.CustomerAccountService.Application.Features.AdminCustomers.Services;

public sealed class AdminCustomerAppService : IAdminCustomerAppService, IScopeDependency
{
    private readonly ICustomerCommandRepository _commandRepository;
    private readonly IAdminCustomerQueryRepository _queryRepository;
    private readonly ICustomerAccountUnitOfWork _unitOfWork;

    public AdminCustomerAppService(
        ICustomerCommandRepository commandRepository,
        IAdminCustomerQueryRepository queryRepository,
        ICustomerAccountUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<AdminCustomerBasicResponse>>> GetPagedAsync(
        GetPagedAdminCustomersRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminCustomerRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<PagedResult<AdminCustomerBasicResponse>>(validationError);
        }

        var pagedResult = await _queryRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(pagedResult);
    }

    public async Task<Result<AdminCustomerDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _queryRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<AdminCustomerDetailResponse>(Error.NotFound($"Customer '{id}' was not found."));
        }

        return Result.Success(customer);
    }

    public Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        return ChangeActivationAsync(id, true, cancellationToken);
    }

    public Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        return ChangeActivationAsync(id, false, cancellationToken);
    }

    private async Task<Result> ChangeActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var customer = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(Error.NotFound($"Customer '{id}' was not found."));
        }

        if (customer.IsActive == isActive)
        {
            return Result.Success();
        }

        if (isActive)
        {
            customer.Activate();
        }
        else
        {
            customer.Deactivate();
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

