using Explore.AdminIdentityService.Domain.AdminRoles;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminRoleCommandRepository
{
    Task<AdminRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AdminRole>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task AddAsync(AdminRole adminRole, CancellationToken cancellationToken);

    Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken);

    void Remove(AdminRole adminRole);
}

