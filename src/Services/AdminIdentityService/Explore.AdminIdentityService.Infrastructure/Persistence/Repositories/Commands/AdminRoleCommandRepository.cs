using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Domain.AdminRoles;
using Microsoft.EntityFrameworkCore;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Commands;

public sealed class AdminRoleCommandRepository : IAdminRoleCommandRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminRoleCommandRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.AdminRoles.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminRole>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.AdminRoles
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(AdminRole adminRole, CancellationToken cancellationToken)
    {
        return _dbContext.AdminRoles.AddAsync(adminRole, cancellationToken).AsTask();
    }

    public Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminRoles.AnyAsync(
            x => x.Code == code && (!excludedId.HasValue || x.Id != excludedId.Value),
            cancellationToken);
    }

    public void Remove(AdminRole adminRole)
    {
        _dbContext.AdminRoles.Remove(adminRole);
    }
}

