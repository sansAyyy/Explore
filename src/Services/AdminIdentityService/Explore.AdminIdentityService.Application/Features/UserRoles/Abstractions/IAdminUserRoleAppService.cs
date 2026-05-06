using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.UserRoles.Abstractions;

public interface IAdminUserRoleAppService
{
    Task<Result<AdminUserRolesResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<AdminUserRolesResponse>> AssignAsync(Guid userId, AssignUserRolesRequest request, CancellationToken cancellationToken);
}

