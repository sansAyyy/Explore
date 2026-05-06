using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.Permissions.Abstractions;

public interface IAdminPermissionAppService
{
    Task<Result<PagedResult<AdminPermissionBasicResponse>>> GetPagedAsync(GetPagedAdminPermissionsRequest request, CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<AdminPermissionBasicResponse>>> GetRootsAsync(GetAdminPermissionTreeRequest request, CancellationToken cancellationToken);

    Task<Result<AdminPermissionTreeNodeResponse>> GetDescendantsAsync(Guid id, GetAdminPermissionTreeRequest request, CancellationToken cancellationToken);

    Task<Result<AdminPermissionDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<AdminPermissionDetailResponse>> CreateAsync(CreateAdminPermissionRequest request, CancellationToken cancellationToken);

    Task<Result<AdminPermissionDetailResponse>> UpdateAsync(Guid id, UpdateAdminPermissionRequest request, CancellationToken cancellationToken);

    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

