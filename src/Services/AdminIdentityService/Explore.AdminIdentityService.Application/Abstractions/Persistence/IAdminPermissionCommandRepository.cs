using Explore.AdminIdentityService.Domain.AdminPermissions;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminPermissionCommandRepository
{
    Task<AdminPermission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AdminPermission>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task<bool> ExistsByParentIdAsync(Guid parentId, CancellationToken cancellationToken);

    Task AddAsync(AdminPermission adminPermission, CancellationToken cancellationToken);

    Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken);

    void Remove(AdminPermission adminPermission);
}

