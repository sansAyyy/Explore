using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Authorization.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.Authorization.Services;

public sealed class AdminAuthorizationAppService : IAdminAuthorizationAppService, IScopeDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IAdminUserQueryRepository _adminUserQueryRepository;
    private readonly IAdminAuthorizationQueryRepository _adminAuthorizationQueryRepository;

    public AdminAuthorizationAppService(
        ICurrentUser currentUser,
        IAdminUserQueryRepository adminUserQueryRepository,
        IAdminAuthorizationQueryRepository adminAuthorizationQueryRepository)
    {
        _currentUser = currentUser;
        _adminUserQueryRepository = adminUserQueryRepository;
        _adminAuthorizationQueryRepository = adminAuthorizationQueryRepository;
    }

    public async Task<Result<CurrentAdminAuthorizationResponse>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<CurrentAdminAuthorizationResponse>(Error.Unauthorized("Current admin user is not authenticated."));
        }

        var adminUser = await _adminUserQueryRepository.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<CurrentAdminAuthorizationResponse>(Error.NotFound("Current admin user was not found."));
        }

        if (!adminUser.IsActive)
        {
            return Result.Failure<CurrentAdminAuthorizationResponse>(Error.Forbidden("Current admin user is disabled."));
        }

        var authorization = await _adminAuthorizationQueryRepository.GetCurrentAsync(_currentUser.UserId.Value, cancellationToken);
        return authorization is null
            ? Result.Failure<CurrentAdminAuthorizationResponse>(Error.NotFound("Current admin authorization was not found."))
            : Result.Success(authorization);
    }
}

