using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Security.Authentication.Abstractions;
using BuildingBlocks.Security.Authentication.Constants;
using BuildingBlocks.Security.Hashing.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Models;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Authentication.Abstractions;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.Authentication.Validators;
using Explore.AdminIdentityService.Domain.AdminUsers;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Services;

public sealed class AdminAuthenticationAppService : IAdminAuthenticationAppService, IScopeDependency
{
    private const string PhoneVerificationScope = "admin-auth";

    private readonly IAdminUserCommandRepository _commandRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAdminMessageCenterClient _messageCenterClient;
    private readonly IAdminRefreshTokenService _refreshTokenService;
    private readonly IPhoneVerificationCodeService _phoneVerificationCodeService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAdminIdentityUnitOfWork _unitOfWork;

    public AdminAuthenticationAppService(
        IAdminUserCommandRepository commandRepository,
        ICurrentUser currentUser,
        IJwtTokenService jwtTokenService,
        IAdminMessageCenterClient messageCenterClient,
        IAdminRefreshTokenService refreshTokenService,
        IPhoneVerificationCodeService phoneVerificationCodeService,
        IPasswordHashService passwordHashService,
        IAdminIdentityUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _currentUser = currentUser;
        _jwtTokenService = jwtTokenService;
        _messageCenterClient = messageCenterClient;
        _refreshTokenService = refreshTokenService;
        _phoneVerificationCodeService = phoneVerificationCodeService;
        _passwordHashService = passwordHashService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminLoginResponse>> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminLoginResponse>(validationError);
        }

        var adminUser = await _commandRepository.GetByAccountAsync(request.Account.Trim(), cancellationToken);
        if (adminUser is null || !_passwordHashService.VerifyHashedPassword(adminUser.PasswordHash, request.Password))
        {
            return Result.Failure<AdminLoginResponse>(Error.Unauthorized("Account or password is invalid."));
        }

        if (!adminUser.IsActive)
        {
            return Result.Failure<AdminLoginResponse>(Error.Forbidden("Admin user is disabled."));
        }

        adminUser.MarkLogin(DateTime.UtcNow);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(await CreateLoginResponseAsync(adminUser, cancellationToken));
    }

    public async Task<Result> SendPhoneLoginCodeAsync(
        AdminSendPhoneLoginCodeRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        var phoneNumber = request.PhoneNumber.Trim();
        var adminUser = await _commandRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (adminUser is null || !adminUser.IsActive)
        {
            return Result.Success();
        }

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
            issueResult.Value.ExpiresAt - DateTime.UtcNow,
            cancellationToken);
        if (sendResult.IsFailure)
        {
            await _phoneVerificationCodeService.InvalidateAsync(PhoneVerificationScope, phoneNumber, cancellationToken);
            return sendResult;
        }

        return Result.Success();
    }

    public async Task<Result<AdminLoginResponse>> PhoneLoginAsync(
        AdminPhoneLoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminLoginResponse>(validationError);
        }

        var phoneNumber = request.PhoneNumber.Trim();
        var adminUser = await _commandRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<AdminLoginResponse>(Error.BadRequest("Phone number is not bound to any admin user."));
        }

        if (!adminUser.IsActive)
        {
            return Result.Failure<AdminLoginResponse>(Error.Forbidden("Admin user is disabled."));
        }

        var verifyStatus = await _phoneVerificationCodeService.VerifyAsync(
            PhoneVerificationScope,
            phoneNumber,
            request.VerificationCode,
            cancellationToken);

        if (verifyStatus == PhoneVerificationCodeVerificationStatus.MissingOrExpired ||
            verifyStatus == PhoneVerificationCodeVerificationStatus.AttemptsExceeded)
        {
            return Result.Failure<AdminLoginResponse>(Error.Unauthorized("Verification code has expired or was not requested."));
        }

        if (verifyStatus == PhoneVerificationCodeVerificationStatus.Invalid)
        {
            return Result.Failure<AdminLoginResponse>(Error.Unauthorized("Verification code is invalid."));
        }

        adminUser.MarkLogin(DateTime.UtcNow);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(await CreateLoginResponseAsync(adminUser, cancellationToken));
    }

    public async Task<Result<AdminLoginResponse>> RefreshAsync(
        AdminRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminLoginResponse>(validationError);
        }

        var validatedRefreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);
        if (validatedRefreshToken.IsFailure)
        {
            return Result.Failure<AdminLoginResponse>(MapRefreshTokenError(validatedRefreshToken.Error));
        }

        var adminUser = await _commandRepository.GetByIdAsync(validatedRefreshToken.Value!.UserId, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<AdminLoginResponse>(Error.Unauthorized("Refresh token is invalid."));
        }

        if (!adminUser.IsActive)
        {
            return Result.Failure<AdminLoginResponse>(Error.Forbidden("Admin user is disabled."));
        }

        var accessToken = _jwtTokenService.CreateAccessToken(adminUser.Id.ToString(), adminUser.UserName, TokenParties.Admin);
        var rotatedRefreshToken = await _refreshTokenService.RotateAsync(validatedRefreshToken.Value, cancellationToken);

        return Result.Success(new AdminLoginResponse(
            accessToken.AccessToken,
            rotatedRefreshToken.RefreshToken,
            "Bearer",
            accessToken.ExpiresAt,
            rotatedRefreshToken.ExpiresAt));
    }

    public async Task<Result> LogoutAsync(
        AdminLogoutRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminAuthenticationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure(Error.Unauthorized("Current admin user is not authenticated."));
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
            return Result.Failure(Error.Forbidden("Refresh token does not belong to the current admin user."));
        }

        await _refreshTokenService.RevokeAsync(validatedRefreshToken.Value.SessionId, cancellationToken);
        return Result.Success();
    }

    private async Task<AdminLoginResponse> CreateLoginResponseAsync(AdminUser adminUser, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.CreateAccessToken(adminUser.Id.ToString(), adminUser.UserName, TokenParties.Admin);
        var refreshToken = await _refreshTokenService.CreateAsync(adminUser.Id, cancellationToken);

        return new AdminLoginResponse(
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

