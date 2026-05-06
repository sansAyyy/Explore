using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.Roles.Abstractions;

public interface IAdminRoleAppService
{
    Task<Result<PagedResult<AdminRoleBasicResponse>>> GetPagedAsync(GetPagedAdminRolesRequest request, CancellationToken cancellationToken);

    Task<Result<AdminRoleDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<AdminRoleDetailResponse>> CreateAsync(CreateAdminRoleRequest request, CancellationToken cancellationToken);

    Task<Result<AdminRoleDetailResponse>> UpdateAsync(Guid id, UpdateAdminRoleRequest request, CancellationToken cancellationToken);

    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<AdminRolePermissionsResponse>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken);

    Task<Result<AdminRolePermissionsResponse>> AssignPermissionsAsync(Guid roleId, AssignRolePermissionsRequest request, CancellationToken cancellationToken);
}

