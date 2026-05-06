using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Permissions.Abstractions;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.Permissions.Validators;
using Explore.AdminIdentityService.Domain.AdminPermissions;

namespace Explore.AdminIdentityService.Application.Features.Permissions.Services;

public sealed class AdminPermissionAppService : IAdminPermissionAppService, IScopeDependency
{
    private readonly IAdminPermissionCommandRepository _permissionCommandRepository;
    private readonly IAdminPermissionQueryRepository _permissionQueryRepository;
    private readonly IAdminRolePermissionCommandRepository _rolePermissionCommandRepository;
    private readonly IAdminIdentityUnitOfWork _unitOfWork;

    public AdminPermissionAppService(
        IAdminPermissionCommandRepository permissionCommandRepository,
        IAdminPermissionQueryRepository permissionQueryRepository,
        IAdminRolePermissionCommandRepository rolePermissionCommandRepository,
        IAdminIdentityUnitOfWork unitOfWork)
    {
        _permissionCommandRepository = permissionCommandRepository;
        _permissionQueryRepository = permissionQueryRepository;
        _rolePermissionCommandRepository = rolePermissionCommandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<AdminPermissionBasicResponse>>> GetPagedAsync(GetPagedAdminPermissionsRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminPermissionRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<PagedResult<AdminPermissionBasicResponse>>(validationError);
        }

        return Result.Success(await _permissionQueryRepository.GetPagedAsync(request, cancellationToken));
    }

    public async Task<Result<IReadOnlyCollection<AdminPermissionBasicResponse>>> GetRootsAsync(GetAdminPermissionTreeRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminPermissionRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<IReadOnlyCollection<AdminPermissionBasicResponse>>(validationError);
        }

        return Result.Success(await _permissionQueryRepository.GetRootsAsync(request.IsActive, cancellationToken));
    }

    public async Task<Result<AdminPermissionTreeNodeResponse>> GetDescendantsAsync(Guid id, GetAdminPermissionTreeRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminPermissionRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminPermissionTreeNodeResponse>(validationError);
        }

        var tree = await _permissionQueryRepository.GetDescendantsAsync(id, request.IsActive, cancellationToken);
        return tree is null
            ? Result.Failure<AdminPermissionTreeNodeResponse>(Error.NotFound($"Top-level admin permission '{id}' was not found."))
            : Result.Success(tree);
    }

    public async Task<Result<AdminPermissionDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await _permissionQueryRepository.GetByIdAsync(id, cancellationToken);
        return permission is null
            ? Result.Failure<AdminPermissionDetailResponse>(Error.NotFound($"Admin permission '{id}' was not found."))
            : Result.Success(permission);
    }

    public async Task<Result<AdminPermissionDetailResponse>> CreateAsync(CreateAdminPermissionRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminPermissionRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminPermissionDetailResponse>(validationError);
        }

        var uniquenessError = await ValidateUniquenessAsync(request.Code, null, cancellationToken);
        if (uniquenessError is not null)
        {
            return Result.Failure<AdminPermissionDetailResponse>(uniquenessError);
        }

        var parentValidationError = await ValidateParentAsync(request.ParentId, null, cancellationToken);
        if (parentValidationError is not null)
        {
            return Result.Failure<AdminPermissionDetailResponse>(parentValidationError);
        }

        try
        {
            var permission = AdminPermission.Create(
                Guid.NewGuid(),
                request.ParentId,
                request.Code,
                request.Name,
                request.Description,
                request.ResourceType,
                request.IsActive);

            await _permissionCommandRepository.AddAsync(permission, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return await LoadDetailAsync(permission.Id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<AdminPermissionDetailResponse>(Error.Validation(exception.Message));
        }
    }

    public async Task<Result<AdminPermissionDetailResponse>> UpdateAsync(Guid id, UpdateAdminPermissionRequest request, CancellationToken cancellationToken)
    {
        var validationError = AdminPermissionRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<AdminPermissionDetailResponse>(validationError);
        }

        var permission = await _permissionCommandRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return Result.Failure<AdminPermissionDetailResponse>(Error.NotFound($"Admin permission '{id}' was not found."));
        }

        var uniquenessError = await ValidateUniquenessAsync(request.Code, id, cancellationToken);
        if (uniquenessError is not null)
        {
            return Result.Failure<AdminPermissionDetailResponse>(uniquenessError);
        }

        var parentValidationError = await ValidateParentAsync(request.ParentId, id, cancellationToken);
        if (parentValidationError is not null)
        {
            return Result.Failure<AdminPermissionDetailResponse>(parentValidationError);
        }

        try
        {
            permission.Update(
                request.ParentId,
                request.Code,
                request.Name,
                request.Description,
                request.ResourceType);

            await _unitOfWork.CommitAsync(cancellationToken);
            return await LoadDetailAsync(id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<AdminPermissionDetailResponse>(Error.Validation(exception.Message));
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
        var permission = await _permissionCommandRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return Result.Failure(Error.NotFound($"Admin permission '{id}' was not found."));
        }

        if (await _rolePermissionCommandRepository.ExistsByPermissionIdAsync(id, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Admin permission is assigned to roles and cannot be deleted."));
        }

        if (await _permissionCommandRepository.ExistsByParentIdAsync(id, cancellationToken))
        {
            return Result.Failure(Error.Conflict("Admin permission has child permissions and cannot be deleted."));
        }

        _permissionCommandRepository.Remove(permission);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> ChangeActivationAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var permission = await _permissionCommandRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return Result.Failure(Error.NotFound($"Admin permission '{id}' was not found."));
        }

        if (permission.IsActive == isActive)
        {
            return Result.Success();
        }

        if (isActive)
        {
            permission.Activate();
        }
        else
        {
            permission.Deactivate();
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Error?> ValidateUniquenessAsync(
        string code,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        var trimmedCode = code.Trim();
        if (await _permissionCommandRepository.ExistsByCodeAsync(trimmedCode, excludedId, cancellationToken))
        {
            return Error.Conflict($"Code '{trimmedCode}' already exists.");
        }

        return null;
    }

    private async Task<Error?> ValidateParentAsync(Guid? parentId, Guid? currentPermissionId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return null;
        }

        if (currentPermissionId.HasValue && parentId.Value == currentPermissionId.Value)
        {
            return Error.Validation("ParentId cannot reference itself.");
        }

        var visited = new HashSet<Guid>();
        var currentParentId = parentId;

        while (currentParentId.HasValue)
        {
            if (!visited.Add(currentParentId.Value))
            {
                return Error.Validation("ParentId contains a cycle.");
            }

            if (currentPermissionId.HasValue && currentParentId.Value == currentPermissionId.Value)
            {
                return Error.Validation("ParentId contains a cycle.");
            }

            var parent = await _permissionCommandRepository.GetByIdAsync(currentParentId.Value, cancellationToken);
            if (parent is null)
            {
                return Error.Validation("ParentId contains unknown permission.");
            }

            currentParentId = parent.ParentId;
        }

        return null;
    }

    private async Task<Result<AdminPermissionDetailResponse>> LoadDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await _permissionQueryRepository.GetByIdAsync(id, cancellationToken);
        return permission is null
            ? Result.Failure<AdminPermissionDetailResponse>(Error.NotFound($"Admin permission '{id}' was not found."))
            : Result.Success(permission);
    }
}

