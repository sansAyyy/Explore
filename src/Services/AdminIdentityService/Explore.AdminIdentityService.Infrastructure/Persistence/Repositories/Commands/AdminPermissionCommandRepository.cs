using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Domain.AdminPermissions;
using Microsoft.EntityFrameworkCore;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Commands;

public sealed class AdminPermissionCommandRepository : IAdminPermissionCommandRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminPermissionCommandRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminPermission?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.AdminPermissions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminPermission>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.AdminPermissions
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(AdminPermission adminPermission, CancellationToken cancellationToken)
    {
        return _dbContext.AdminPermissions.AddAsync(adminPermission, cancellationToken).AsTask();
    }

    public Task<bool> ExistsByParentIdAsync(Guid parentId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminPermissions.AnyAsync(x => x.ParentId == parentId, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, Guid? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminPermissions.AnyAsync(
            x => x.Code == code && (!excludedId.HasValue || x.Id != excludedId.Value),
            cancellationToken);
    }

    public void Remove(AdminPermission adminPermission)
    {
        _dbContext.AdminPermissions.Remove(adminPermission);
    }
}

