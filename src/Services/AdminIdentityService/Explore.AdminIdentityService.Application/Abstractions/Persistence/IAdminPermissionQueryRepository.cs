using BuildingBlocks.Common.Pagination;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminPermissionQueryRepository
{
    Task<IReadOnlyCollection<AdminPermissionBasicResponse>> GetRootsAsync(bool? isActive, CancellationToken cancellationToken);

    Task<AdminPermissionTreeNodeResponse?> GetDescendantsAsync(Guid rootId, bool? isActive, CancellationToken cancellationToken);

    Task<AdminPermissionDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<AdminPermissionBasicResponse>> GetPagedAsync(GetPagedAdminPermissionsRequest request, CancellationToken cancellationToken);
}

