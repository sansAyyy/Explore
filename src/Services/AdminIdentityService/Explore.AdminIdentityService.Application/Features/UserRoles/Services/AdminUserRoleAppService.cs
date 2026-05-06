using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.UserRoles.Abstractions;
using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.UserRoles.Validators;
using Explore.AdminIdentityService.Domain.AdminUserRoles;

namespace Explore.AdminIdentityService.Application.Features.UserRoles.Services;

public sealed class AdminUserRoleAppService : IAdminUserRoleAppService, IScopeDependency
{
    private readonly IAdminUserCommandRepository _adminUserCommandRepository;
    private readonly IAdminRoleCommandRepository _adminRoleCommandRepository;
    private readonly IAdminUserRoleCommandRepository _adminUserRoleCommandRepository;
    private readonly IAdminUserRoleQueryRepository _adminUserRoleQueryRepository;
    private readonly IAdminIdentityUnitOfWork _unitOfWork;

    public AdminUserRoleAppService(
        IAdminUserCommandRepository adminUserCommandRepository,
        IAdminRoleCommandRepository adminRoleCommandRepository,
        IAdminUserRoleCommandRepository adminUserRoleCommandRepository,
        IAdminUserRoleQueryRepository adminUserRoleQueryRepository,
        IAdminIdentityUnitOfWork unitOfWork)
    {
        _adminUserCommandRepository = adminUserCommandRepository;
        _adminRoleCommandRepository = adminRoleCommandRepository;
        _adminUserRoleCommandRepository = adminUserRoleCommandRepository;
        _adminUserRoleQueryRepository = adminUserRoleQueryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminUserRolesResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var adminUser = await _adminUserCommandRepository.GetByIdAsync(userId, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<AdminUserRolesResponse>(Error.NotFound($"Admin user '{userId}' was not found."));
        }

        var roles = await _adminUserRoleQueryRepository.GetByUserIdAsync(userId, cancellationToken);
        return Result.Success(roles ?? new AdminUserRolesResponse(userId, []));
    }

    public async Task<Result<AdminUserRolesResponse>> AssignAsync(
        Guid userId,
        AssignUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminUserRoleRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminUserRolesResponse>(validationError);
        }

        var adminUser = await _adminUserCommandRepository.GetByIdAsync(userId, cancellationToken);
        if (adminUser is null)
        {
            return Result.Failure<AdminUserRolesResponse>(Error.NotFound($"Admin user '{userId}' was not found."));
        }

        var distinctRoleIds = request.RoleIds.Distinct().ToArray();
        if (distinctRoleIds.Length > 0)
        {
            var roles = await _adminRoleCommandRepository.GetByIdsAsync(distinctRoleIds, cancellationToken);
            if (roles.Count != distinctRoleIds.Length)
            {
                return Result.Failure<AdminUserRolesResponse>(Error.Validation("RoleIds contains unknown role."));
            }

            if (roles.Any(x => !x.IsActive))
            {
                return Result.Failure<AdminUserRolesResponse>(Error.Validation("Inactive roles cannot be assigned."));
            }
        }

        var existingAssignments = await _adminUserRoleCommandRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingAssignments.Count > 0)
        {
            _adminUserRoleCommandRepository.RemoveRange(existingAssignments);
        }

        var newAssignments = distinctRoleIds
            .Select(roleId => AdminUserRole.Create(Guid.NewGuid(), userId, roleId))
            .ToArray();

        if (newAssignments.Length > 0)
        {
            await _adminUserRoleCommandRepository.AddRangeAsync(newAssignments, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return await GetByUserIdAsync(userId, cancellationToken);
    }
}

