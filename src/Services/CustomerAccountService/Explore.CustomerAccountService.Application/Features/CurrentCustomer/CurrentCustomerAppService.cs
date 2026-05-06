using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;

namespace Explore.CustomerAccountService.Application.Features.CurrentCustomer;

public sealed class CurrentCustomerAppService : ICurrentCustomerAppService, IScopeDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly ICustomerCommandRepository _commandRepository;
    private readonly ICustomerAccountUnitOfWork _unitOfWork;

    public CurrentCustomerAppService(
        ICurrentUser currentUser,
        ICustomerCommandRepository commandRepository,
        ICustomerAccountUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _commandRepository = commandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CurrentCustomerResponse>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var customerResult = await GetAuthenticatedCustomerAsync(cancellationToken);
        if (customerResult.IsFailure)
        {
            return Result.Failure<CurrentCustomerResponse>(customerResult.Error);
        }

        return Result.Success(Map(customerResult.Value!));
    }

    public async Task<Result<CurrentCustomerResponse>> UpdateProfileAsync(
        UpdateCurrentCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CurrentCustomerRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<CurrentCustomerResponse>(validationError);
        }

        var customerResult = await GetAuthenticatedCustomerAsync(cancellationToken);
        if (customerResult.IsFailure)
        {
            return Result.Failure<CurrentCustomerResponse>(customerResult.Error);
        }

        var customer = customerResult.Value!;

        var trimmedEmail = request.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedEmail) &&
            await _commandRepository.ExistsByEmailAsync(trimmedEmail, customer.Id, cancellationToken))
        {
            return Result.Failure<CurrentCustomerResponse>(Error.Conflict($"Email '{trimmedEmail}' already exists."));
        }

        try
        {
            customer.UpdateProfile(request.NickName, request.AvatarUrl, trimmedEmail);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<CurrentCustomerResponse>(Error.Validation(exception.Message));
        }

        return Result.Success(Map(customer));
    }

    public async Task<Result<CurrentCustomerResponse>> UpdateAvatarAsync(
        UpdateCurrentCustomerAvatarRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CurrentCustomerRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<CurrentCustomerResponse>(validationError);
        }

        var customerResult = await GetAuthenticatedCustomerAsync(cancellationToken);
        if (customerResult.IsFailure)
        {
            return Result.Failure<CurrentCustomerResponse>(customerResult.Error);
        }

        var customer = customerResult.Value!;
        var avatarUrl = NormalizeOptionalField(request.AvatarUrl);

        try
        {
            customer.UpdateProfile(customer.NickName, avatarUrl, customer.Email);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<CurrentCustomerResponse>(Error.Validation(exception.Message));
        }

        return Result.Success(Map(customer));
    }

    private async Task<Result<Domain.Customers.Customer>> GetAuthenticatedCustomerAsync(CancellationToken cancellationToken)
    {
        var currentCustomerId = _currentUser.UserId;
        if (!currentCustomerId.HasValue)
        {
            return Result.Failure<Domain.Customers.Customer>(Error.Unauthorized("Current customer is not authenticated."));
        }

        var customer = await _commandRepository.GetByIdAsync(currentCustomerId.Value, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<Domain.Customers.Customer>(Error.NotFound("Current customer was not found."));
        }

        if (!customer.IsActive)
        {
            return Result.Failure<Domain.Customers.Customer>(Error.Forbidden("Current customer is disabled."));
        }

        return Result.Success(customer);
    }

    private static string? NormalizeOptionalField(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static CurrentCustomerResponse Map(Domain.Customers.Customer customer)
    {
        return new CurrentCustomerResponse(
            customer.Id,
            customer.PhoneNumber,
            customer.NickName,
            customer.AvatarUrl,
            customer.Email,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt,
            customer.LastLoginAt,
            customer.Version);
    }
}

