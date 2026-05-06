using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Security.Hashing.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Abstractions;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Validators;

namespace Explore.AdminIdentityService.Application.Features.CurrentAdmin.Services;

public sealed class CurrentAdminAppService : ICurrentAdminAppService, IScopeDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IAdminUserCommandRepository _commandRepository;
    private readonly IAdminUserQueryRepository _queryRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAdminIdentityUnitOfWork _unitOfWork;

    public CurrentAdminAppService(
        ICurrentUser currentUser,
        IAdminUserCommandRepository commandRepository,
        IAdminUserQueryRepository queryRepository,
        IPasswordHashService passwordHashService,
        IAdminIdentityUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _passwordHashService = passwordHashService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CurrentAdminResponse>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentAdminId();
        if (!currentAdminId.HasValue)
        {
            return Result.Failure<CurrentAdminResponse>(Error.Unauthorized("Current admin user is not authenticated."));
        }

        var adminUser = await _queryRepository.GetByIdAsync(currentAdminId.Value, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<CurrentAdminResponse>(Error.NotFound("Current admin user was not found."));
        }

        if (!adminUser.IsActive)
        {
            return Result.Failure<CurrentAdminResponse>(Error.Forbidden("Current admin user is disabled."));
        }

        return Result.Success(Map(adminUser));
    }

    public async Task<Result<CurrentAdminResponse>> UpdateProfileAsync(
        UpdateCurrentAdminProfileRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CurrentAdminRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<CurrentAdminResponse>(validationError);
        }

        var currentAdminId = GetCurrentAdminId();
        if (!currentAdminId.HasValue)
        {
            return Result.Failure<CurrentAdminResponse>(Error.Unauthorized("Current admin user is not authenticated."));
        }

        var adminUser = await _commandRepository.GetByIdAsync(currentAdminId.Value, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<CurrentAdminResponse>(Error.NotFound("Current admin user was not found."));
        }

        if (!adminUser.IsActive)
        {
            return Result.Failure<CurrentAdminResponse>(Error.Forbidden("Current admin user is disabled."));
        }

        var trimmedUserName = request.UserName.Trim();
        if (await _commandRepository.ExistsByUserNameAsync(trimmedUserName, adminUser.Id, cancellationToken))
        {
            return Result.Failure<CurrentAdminResponse>(Error.Conflict($"UserName '{trimmedUserName}' already exists."));
        }

        var trimmedEmail = request.Email.Trim();
        if (await _commandRepository.ExistsByEmailAsync(trimmedEmail, adminUser.Id, cancellationToken))
        {
            return Result.Failure<CurrentAdminResponse>(Error.Conflict($"Email '{trimmedEmail}' already exists."));
        }

        try
        {
            adminUser.UpdateProfile(request.UserName, request.Email, request.DisplayName, adminUser.PhoneNumber);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<CurrentAdminResponse>(Error.Validation(exception.Message));
        }

        var updatedAdminUser = await _queryRepository.GetByIdAsync(adminUser.Id, cancellationToken);
        if (updatedAdminUser is null)
        {
            return Result.Failure<CurrentAdminResponse>(Error.NotFound("Current admin user was not found."));
        }

        return Result.Success(Map(updatedAdminUser));
    }

    public async Task<Result> ChangePasswordAsync(
        ChangeCurrentAdminPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = CurrentAdminRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        var currentAdminId = GetCurrentAdminId();
        if (!currentAdminId.HasValue)
        {
            return Result.Failure(Error.Unauthorized("Current admin user is not authenticated."));
        }

        var adminUser = await _commandRepository.GetByIdAsync(currentAdminId.Value, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure(Error.NotFound("Current admin user was not found."));
        }

        if (!adminUser.IsActive)
        {
            return Result.Failure(Error.Forbidden("Current admin user is disabled."));
        }

        if (!_passwordHashService.VerifyHashedPassword(adminUser.PasswordHash, request.CurrentPassword))
        {
            return Result.Failure(Error.Validation("CurrentPassword is incorrect."));
        }

        if (_passwordHashService.VerifyHashedPassword(adminUser.PasswordHash, request.NewPassword))
        {
            return Result.Failure(Error.Validation("NewPassword must be different from the current password."));
        }

        try
        {
            adminUser.ChangePassword(_passwordHashService.HashPassword(request.NewPassword));
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(Error.Validation(exception.Message));
        }
    }

    private Guid? GetCurrentAdminId()
    {
        return _currentUser.UserId;
    }

    private static CurrentAdminResponse Map(AdminUsers.Dtos.Responses.AdminUserDetailResponse adminUser)
    {
        return new CurrentAdminResponse(
            adminUser.Id,
            adminUser.UserName,
            adminUser.Email,
            adminUser.PhoneNumber,
            adminUser.DisplayName,
            adminUser.IsActive,
            adminUser.CreatedAt,
            adminUser.UpdatedAt,
            adminUser.LastLoginAt,
            adminUser.Version);
    }
}

