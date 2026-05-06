using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Security.Hashing.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Abstractions;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Validators;
using Explore.AdminIdentityService.Domain.AdminUsers;
using Microsoft.Extensions.Logging;

namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Services;

public sealed class AdminUserAppService : IAdminUserAppService, IScopeDependency
{
    private readonly IAdminUserCommandRepository _commandRepository;
    private readonly IAdminUserQueryRepository _queryRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ICurrentUser _currentUser;
    private readonly IAdminSiteMessageSender _siteMessageSender;
    private readonly ILogger<AdminUserAppService> _logger;
    private readonly IAdminIdentityUnitOfWork _unitOfWork;

    public AdminUserAppService(
        IAdminUserCommandRepository commandRepository,
        IAdminUserQueryRepository queryRepository,
        IPasswordHashService passwordHashService,
        ICurrentUser currentUser,
        IAdminSiteMessageSender siteMessageSender,
        ILogger<AdminUserAppService> logger,
        IAdminIdentityUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _passwordHashService = passwordHashService;
        _currentUser = currentUser;
        _siteMessageSender = siteMessageSender;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<AdminUserBasicResponse>>> GetPagedAsync(
        GetPagedAdminUsersRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminUserRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<PagedResult<AdminUserBasicResponse>>(validationError);
        }

        var pagedResult = await _queryRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(pagedResult);
    }

    public async Task<Result<AdminUserDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var adminUser = await _queryRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<AdminUserDetailResponse>(Error.NotFound($"Admin user '{id}' was not found."));
        }

        return Result.Success(adminUser);
    }

    public async Task<Result<AdminUserDetailResponse>> CreateAsync(
        CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminUserRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminUserDetailResponse>(validationError);
        }

        var conflictError = await ValidateUniquenessAsync(request.UserName, request.Email, request.PhoneNumber, null, cancellationToken);
        if (conflictError is not null)
        {
            return Result.Failure<AdminUserDetailResponse>(conflictError);
        }

        try
        {
            var adminUser = AdminUser.Create(
                Guid.NewGuid(),
                request.UserName,
                request.Email,
                request.DisplayName,
                request.PhoneNumber,
                _passwordHashService.HashPassword(request.Password),
                request.IsActive);

            await _commandRepository.AddAsync(adminUser, cancellationToken);
            var sendMessageResult = await TrySendSiteMessageAsync(
                AdminIdentitySiteMessageTemplateCodes.AdminUserCreated,
                adminUser,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["adminUserId"] = adminUser.Id.ToString(),
                    ["userName"] = adminUser.UserName,
                    ["displayName"] = adminUser.DisplayName,
                    ["operatorAdminUserId"] = ResolveOperatorAdminUserId(),
                    ["email"] = adminUser.Email
                },
                $"admin_identity:admin_user_created:{adminUser.Id}:1",
                cancellationToken);
            if (sendMessageResult.IsFailure)
            {
                return Result.Failure<AdminUserDetailResponse>(sendMessageResult.Error);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return await LoadExistingDetailAsync(adminUser.Id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<AdminUserDetailResponse>(Error.Validation(exception.Message));
        }
    }

    public async Task<Result<AdminUserDetailResponse>> UpdateAsync(
        Guid id,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminUserRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminUserDetailResponse>(validationError);
        }

        var adminUser = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<AdminUserDetailResponse>(Error.NotFound($"Admin user '{id}' was not found."));
        }

        var conflictError = await ValidateUniquenessAsync(request.UserName, request.Email, request.PhoneNumber, id, cancellationToken);
        if (conflictError is not null)
        {
            return Result.Failure<AdminUserDetailResponse>(conflictError);
        }

        try
        {
            adminUser.UpdateProfile(request.UserName, request.Email, request.DisplayName, request.PhoneNumber);
            await _unitOfWork.CommitAsync(cancellationToken);

            return await LoadExistingDetailAsync(id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<AdminUserDetailResponse>(Error.Validation(exception.Message));
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var adminUser = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure(Error.NotFound($"Admin user '{id}' was not found."));
        }

        _commandRepository.Remove(adminUser);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        Guid id,
        ChangeAdminUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminUserRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        var adminUser = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure(Error.NotFound($"Admin user '{id}' was not found."));
        }

        if (_passwordHashService.VerifyHashedPassword(adminUser.PasswordHash, request.NewPassword))
        {
            return Result.Failure(Error.Validation("NewPassword must be different from the current password."));
        }

        try
        {
            adminUser.ChangePassword(_passwordHashService.HashPassword(request.NewPassword));
            var sendMessageResult = await TrySendSiteMessageAsync(
                AdminIdentitySiteMessageTemplateCodes.AdminUserPasswordChangedByAdmin,
                adminUser,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["adminUserId"] = adminUser.Id.ToString(),
                    ["userName"] = adminUser.UserName,
                    ["displayName"] = adminUser.DisplayName,
                    ["operatorAdminUserId"] = ResolveOperatorAdminUserId(),
                    ["changedAt"] = DateTime.UtcNow.ToString("O")
                },
                $"admin_identity:admin_user_password_changed_by_admin:{adminUser.Id}:{GetNextCommittedVersion(adminUser)}",
                cancellationToken);
            if (sendMessageResult.IsFailure)
            {
                return sendMessageResult;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(Error.Validation(exception.Message));
        }
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
        var adminUser = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure(Error.NotFound($"Admin user '{id}' was not found."));
        }

        if (adminUser.IsActive == isActive)
        {
            return Result.Success();
        }

        if (isActive)
        {
            adminUser.Activate();
        }
        else
        {
            adminUser.Deactivate();
        }

        var sendMessageResult = await TrySendSiteMessageAsync(
            isActive
                ? AdminIdentitySiteMessageTemplateCodes.AdminUserActivated
                : AdminIdentitySiteMessageTemplateCodes.AdminUserDeactivated,
            adminUser,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["adminUserId"] = adminUser.Id.ToString(),
                ["userName"] = adminUser.UserName,
                ["displayName"] = adminUser.DisplayName,
                ["operatorAdminUserId"] = ResolveOperatorAdminUserId(),
                ["status"] = isActive ? "enabled" : "disabled"
            },
            $"admin_identity:{(isActive ? "admin_user_activated" : "admin_user_deactivated")}:{adminUser.Id}:{GetNextCommittedVersion(adminUser)}",
            cancellationToken);
        if (sendMessageResult.IsFailure)
        {
            return sendMessageResult;
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<AdminUserDetailResponse>> LoadExistingDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var adminUser = await _queryRepository.GetByIdAsync(id, cancellationToken);
        return adminUser is null
            ? Result.Failure<AdminUserDetailResponse>(Error.NotFound($"Admin user '{id}' was not found."))
            : Result.Success(adminUser);
    }

    private async Task<Error?> ValidateUniquenessAsync(
        string userName,
        string email,
        string? phoneNumber,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        var trimmedUserName = userName.Trim();
        if (await _commandRepository.ExistsByUserNameAsync(trimmedUserName, excludedId, cancellationToken))
        {
            return Error.Conflict($"UserName '{trimmedUserName}' already exists.");
        }

        var trimmedEmail = email.Trim();
        if (await _commandRepository.ExistsByEmailAsync(trimmedEmail, excludedId, cancellationToken))
        {
            return Error.Conflict($"Email '{trimmedEmail}' already exists.");
        }

        var trimmedPhoneNumber = phoneNumber?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedPhoneNumber) &&
            await _commandRepository.ExistsByPhoneNumberAsync(trimmedPhoneNumber, excludedId, cancellationToken))
        {
            return Error.Conflict($"PhoneNumber '{trimmedPhoneNumber}' already exists.");
        }

        return null;
    }

    private async Task<Result> TrySendSiteMessageAsync(
        string templateCode,
        AdminUser adminUser,
        IReadOnlyDictionary<string, string> parameters,
        string businessIdempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await _siteMessageSender.SendAsync(
            new AdminSiteMessageRequest(
                templateCode,
                adminUser.Id,
                parameters,
                businessIdempotencyKey),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Result.Success();
        }

        _logger.LogWarning(
            "Failed to send admin site message. TemplateCode: {TemplateCode}, RecipientUserId: {RecipientUserId}, ActionError: {ErrorMessage}",
            templateCode,
            adminUser.Id,
            result.Error.Message);
        return result;
    }

    private static long GetNextCommittedVersion(AdminUser adminUser)
    {
        return adminUser.Version <= 0 ? 1 : adminUser.Version + 1;
    }

    private string ResolveOperatorAdminUserId()
    {
        return _currentUser.UserId?.ToString() ?? "system";
    }
}

