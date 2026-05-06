using Explore.AdminIdentityService.Domain.AdminUserRoles;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminUserRoleCommandRepository
{
    Task<IReadOnlyCollection<AdminUserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyCollection<AdminUserRole> adminUserRoles, CancellationToken cancellationToken);

    void RemoveRange(IReadOnlyCollection<AdminUserRole> adminUserRoles);

    Task<bool> ExistsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
}

