using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Security.Authentication.Abstractions;
using BuildingBlocks.Security.Authentication.Constants;
using BuildingBlocks.Security.PhoneVerification.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Models;
using Explore.CustomerAccountService.Application.Abstractions.Notifications;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;
using Explore.CustomerAccountService.Domain.Customers;

namespace Explore.CustomerAccountService.Application.Features.Authentication;

public sealed class CustomerAuthenticationAppService : ICustomerAuthenticationAppService, IScopeDependency
{
    private const string PhoneVerificationScope = "customer-auth";

    private readonly ICustomerCommandRepository _commandRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMessageCenterClient _messageCenterClient;
    private readonly IPhoneVerificationCodeService _phoneVerificationCodeService;
    private readonly ICustomerRefreshTokenService _refreshTokenService;
    private readonly ICustomerAccountUnitOfWork _unitOfWork;

    public CustomerAuthenticationAppService(
        ICustomerCommandRepository commandRepository,
        ICurrentUser currentUser,
        IJwtTokenService jwtTokenService,
        IMessageCenterClient messageCenterClient,
        IPhoneVerificationCodeService phoneVerificationCodeService,
        ICustomerRefreshTokenService refreshTokenService,
        ICustomerAccountUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _currentUser = currentUser;
        _jwtTokenService = jwtTokenService;
        _messageCenterClient = messageCenterClient;
        _phoneVerificationCodeService = phoneVerificationCodeService;
        _refreshTokenService = refreshTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> SendPhoneLoginCodeAsync(
        SendCustomerPhoneLoginCodeRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CustomerAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        var phoneNumber = request.PhoneNumber.Trim();
        var issueResult = await _phoneVerificationCodeService.IssueAsync(
            PhoneVerificationScope,
            phoneNumber,
            cancellationToken);
        if (issueResult.IsFailure)
        {
            return Result.Failure(issueResult.Error);
        }

        var sendResult = await _messageCenterClient.SendPhoneLoginCodeAsync(
            phoneNumber,
            issueResult.Value!.Code,
            issueResult.Value!.ExpiresAt - DateTime.UtcNow,
            cancellationToken);
        if (sendResult.IsFailure)
        {
            await _phoneVerificationCodeService.InvalidateAsync(PhoneVerificationScope, phoneNumber, cancellationToken);
            return sendResult;
        }

        return Result.Success();
    }

    public async Task<Result<CustomerLoginResponse>> PhoneLoginAsync(
        CustomerPhoneLoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CustomerAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<CustomerLoginResponse>(validationError);
        }

        var phoneNumber = request.PhoneNumber.Trim();
        var verifyStatus = await _phoneVerificationCodeService.VerifyAsync(
            PhoneVerificationScope,
            phoneNumber,
            request.VerificationCode,
            cancellationToken);
        if (verifyStatus == PhoneVerificationCodeVerificationStatus.MissingOrExpired ||
            verifyStatus == PhoneVerificationCodeVerificationStatus.AttemptsExceeded)
        {
            return Result.Failure<CustomerLoginResponse>(Error.Unauthorized("Verification code has expired or was not requested."));
        }

        if (verifyStatus == PhoneVerificationCodeVerificationStatus.Invalid)
        {
            return Result.Failure<CustomerLoginResponse>(Error.Unauthorized("Verification code is invalid."));
        }

        var customer = await _commandRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken)
            ?? await CreatePhoneLoginCustomerAsync(phoneNumber, cancellationToken);

        if (!customer.IsActive)
        {
            return Result.Failure<CustomerLoginResponse>(Error.Forbidden("Customer is disabled."));
        }

        customer.MarkLogin(DateTime.UtcNow);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(await CreateLoginResponseAsync(customer, cancellationToken));
    }

    public async Task<Result<CustomerLoginResponse>> RefreshAsync(
        CustomerRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CustomerAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<CustomerLoginResponse>(validationError);
        }

        var validatedRefreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);
        if (validatedRefreshToken.IsFailure)
        {
            return Result.Failure<CustomerLoginResponse>(MapRefreshTokenError(validatedRefreshToken.Error));
        }

        var customer = await _commandRepository.GetByIdAsync(validatedRefreshToken.Value!.UserId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<CustomerLoginResponse>(Error.Unauthorized("Refresh token is invalid."));
        }

        if (!customer.IsActive)
        {
            return Result.Failure<CustomerLoginResponse>(Error.Forbidden("Customer is disabled."));
        }

        var accessToken = _jwtTokenService.CreateAccessToken(customer.Id.ToString(), customer.NickName, TokenParties.Customer);
        var rotatedRefreshToken = await _refreshTokenService.RotateAsync(validatedRefreshToken.Value, cancellationToken);

        return Result.Success(new CustomerLoginResponse(
            accessToken.AccessToken,
            rotatedRefreshToken.RefreshToken,
            "Bearer",
            accessToken.ExpiresAt,
            rotatedRefreshToken.ExpiresAt));
    }

    public async Task<Result> LogoutAsync(
        CustomerLogoutRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CustomerAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure(Error.Unauthorized("Current customer is not authenticated."));
        }

        var validatedRefreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);
        if (validatedRefreshToken.IsFailure)
        {
            return validatedRefreshToken.Error.Code == ErrorCodes.ValidationFailed
                ? Result.Failure(validatedRefreshToken.Error)
                : Result.Success();
        }

        if (validatedRefreshToken.Value!.UserId != _currentUser.UserId.Value)
        {
            return Result.Failure(Error.Forbidden("Refresh token does not belong to the current customer."));
        }

        await _refreshTokenService.RevokeAsync(validatedRefreshToken.Value.SessionId, cancellationToken);
        return Result.Success();
    }

    private async Task<Customer> CreatePhoneLoginCustomerAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            phoneNumber,
            $"用户{phoneNumber[^4..]}",
            null,
            null,
            true);

        await _commandRepository.AddAsync(customer, cancellationToken);
        return customer;
    }

    private async Task<CustomerLoginResponse> CreateLoginResponseAsync(Customer customer, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.CreateAccessToken(customer.Id.ToString(), customer.NickName, TokenParties.Customer);
        var refreshToken = await _refreshTokenService.CreateAsync(customer.Id, cancellationToken);

        return new CustomerLoginResponse(
            accessToken.AccessToken,
            refreshToken.RefreshToken,
            "Bearer",
            accessToken.ExpiresAt,
            refreshToken.ExpiresAt);
    }

    private static Error MapRefreshTokenError(Error error)
    {
        return error.Code == ErrorCodes.Forbidden
            ? error
            : Error.Unauthorized("Refresh token is invalid.");
    }
}

