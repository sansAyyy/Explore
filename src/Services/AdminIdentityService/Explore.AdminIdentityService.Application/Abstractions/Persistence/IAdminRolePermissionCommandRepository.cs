using Explore.AdminIdentityService.Domain.AdminRolePermissions;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminRolePermissionCommandRepository
{
    Task<IReadOnlyCollection<AdminRolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyCollection<AdminRolePermission> adminRolePermissions, CancellationToken cancellationToken);

    void RemoveRange(IReadOnlyCollection<AdminRolePermission> adminRolePermissions);

    Task<bool> ExistsByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken);
}

