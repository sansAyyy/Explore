using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Roles.Abstractions;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.Roles.Validators;
using Explore.AdminIdentityService.Domain.AdminRolePermissions;
using Explore.AdminIdentityService.Domain.AdminRoles;

namespace Explore.AdminIdentityService.Application.Features.Roles.Services;

public sealed class AdminRoleAppService : IAdminRoleAppService, IScopeDependency
{
    private readonly IAdminRoleCommandRepository _roleCommandRepository;
    private readonly IAdminRoleQueryRepository _roleQueryRepository;
    private readonly IAdminPermissionCommandRepository _permissionCommandRepository;
    private readonly IAdminRolePermissionCommandRepository _rolePermissionCommandRepository;
    private readonly IAdminUserRoleCommandRepository _userRoleCommandRepository;
    private readonly IAdminIdentityUnitOfWork _unitOfWork;

    public AdminRoleAppService(
        IAdminRoleCommandRepository roleCommandRepository,
        IAdminRoleQueryRepository roleQueryRepository,
        IAdminPermissionCommandRepository permissionCommandRepository,
        IAdminRolePermissionCommandRepository rolePermissionCommandRepository,
        IAdminUserRoleCommandRepository userRoleCommandRepository,
        IAdminIdentityUnitOfWork unitOfWork)
    {
        _roleCommandRepository = roleCommandRepository;
        _roleQueryRepository = roleQueryRepository;
        _permissionCommandRepository = permissionCommandRepository;
        _rolePermissionCommandRepository = rolePermissionCommandRepository;
        _userRoleCommandRepository = userRoleCommandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<AdminRoleBasicResponse>>> GetPagedAsync(GetPagedAdminRolesRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminRoleRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<PagedResult<AdminRoleBasicResponse>>(validationError);
        }

        return Result.Success(await _roleQueryRepository.GetPagedAsync(request, cancellationToken));
    }

    public async Task<Result<AdminRoleDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleQueryRepository.GetByIdAsync(id, cancellationToken);
        return role is null
            ? Result.Failure<AdminRoleDetailResponse>(Error.NotFound($"Admin role '{id}' was not found."))
            : Result.Success(role);
    }

    public async Task<Result<AdminRoleDetailResponse>> CreateAsync(CreateAdminRoleRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminRoleRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminRoleDetailResponse>(validationError);
        }

        var trimmedCode = request.Code.Trim();
        if (await _roleCommandRepository.ExistsByCodeAsync(trimmedCode, null, cancellationToken))
        {
            return Result.Failure<AdminRoleDetailResponse>(Error.Conflict($"Code '{trimmedCode}' already exists."));
        }

        try
        {
            var role = AdminRole.Create(Guid.NewGuid(), request.Code, request.Name, request.Description, request.IsActive);
            await _roleCommandRepository.AddAsync(role, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return await LoadDetailAsync(role.Id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<AdminRoleDetailResponse>(Error.Validation(exception.Message));
        }
    }

    public async Task<Result<AdminRoleDetailResponse>> UpdateAsync(Guid id, UpdateAdminRoleRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminRoleRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminRoleDetailResponse>(validationError);
        }

        var role = await _roleCommandRepository.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return Result.Failure<AdminRoleDetailResponse>(Error.NotFound($"Admin role '{id}' was not found."));
        }

        var trimmedCode = request.Code.Trim();
        if (await _roleCommandRepository.ExistsByCodeAsync(trimmedCode, id, cancellationToken))
        {
            return Result.Failure<AdminRoleDetailResponse>(Error.Conflict($"Code '{trimmedCode}' already exists."));
        }

        try
        {
            role.Update(request.Code, request.Name, request.Description);
            await _unitOfWork.CommitAsync(cancellationToken);
            return await LoadDetailAsync(id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<AdminRoleDetailResponse>(Error.Validation(exception.Message));
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

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleCommandRepository.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return Result.Failure(Error.NotFound($"Admin role '{id}' was not found."));
        }

        if (await _userRoleCommandRepository.ExistsByRoleIdAsync(id, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Admin role is assigned to users and cannot be deleted."));
        }

        var rolePermissions = await _rolePermissionCommandRepository.GetByRoleIdAsync(id, cancellationToken);
        if (rolePermissions.Count > 0)
        {
            _rolePermissionCommandRepository.RemoveRange(rolePermissions);
        }

        _roleCommandRepository.Remove(role);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<AdminRolePermissionsResponse>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var permissions = await _roleQueryRepository.GetPermissionsAsync(roleId, cancellationToken);
        return permissions is null
            ? Result.Failure<AdminRolePermissionsResponse>(Error.NotFound($"Admin role '{roleId}' was not found."))
            : Result.Success(permissions);
    }

    public async Task<Result<AdminRolePermissionsResponse>> AssignPermissionsAsync(
        Guid roleId,
        AssignRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = AdminRoleRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminRolePermissionsResponse>(validationError);
        }

        var role = await _roleCommandRepository.GetByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<AdminRolePermissionsResponse>(Error.NotFound($"Admin role '{roleId}' was not found."));
        }

        var distinctPermissionIds = request.PermissionIds.Distinct().ToArray();
        if (distinctPermissionIds.Length > 0)
        {
            var permissions = await _permissionCommandRepository.GetByIdsAsync(distinctPermissionIds, cancellationToken);
            if (permissions.Count != distinctPermissionIds.Length)
            {
                return Result.Failure<AdminRolePermissionsResponse>(Error.Validation("PermissionIds contains unknown permission."));
            }

            if (permissions.Any(x => !x.IsActive))
            {
                return Result.Failure<AdminRolePermissionsResponse>(Error.Validation("Inactive permissions cannot be assigned."));
            }
        }

        var existingAssignments = await _rolePermissionCommandRepository.GetByRoleIdAsync(roleId, cancellationToken);
        if (existingAssignments.Count > 0)
        {
            _rolePermissionCommandRepository.RemoveRange(existingAssignments);
        }

        var newAssignments = distinctPermissionIds
            .Select(permissionId => AdminRolePermission.Create(Guid.NewGuid(), roleId, permissionId))
            .ToArray();

        if (newAssignments.Length > 0)
        {
            await _rolePermissionCommandRepository.AddRangeAsync(newAssignments, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return await GetPermissionsAsync(roleId, cancellationToken);
    }

    private async Task<Result> ChangeActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var role = await _roleCommandRepository.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return Result.Failure(Error.NotFound($"Admin role '{id}' was not found."));
        }

        if (role.IsActive == isActive)
        {
            return Result.Success();
        }

        if (isActive)
        {
            role.Activate();
        }
        else
        {
            role.Deactivate();
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<AdminRoleDetailResponse>> LoadDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleQueryRepository.GetByIdAsync(id, cancellationToken);
        return role is null
            ? Result.Failure<AdminRoleDetailResponse>(Error.NotFound($"Admin role '{id}' was not found."))
            : Result.Success(role);
    }
}

