using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminUserRoleQueryRepository
{
    Task<AdminUserRolesResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}

