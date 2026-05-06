using BuildingBlocks.Common.Pagination;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminRoleQueryRepository
{
    Task<AdminRoleDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<AdminRoleBasicResponse>> GetPagedAsync(GetPagedAdminRolesRequest request, CancellationToken cancellationToken);

    Task<AdminRolePermissionsResponse?> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken);
}

