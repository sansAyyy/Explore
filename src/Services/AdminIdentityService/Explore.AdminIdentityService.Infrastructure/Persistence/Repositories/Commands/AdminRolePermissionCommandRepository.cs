using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Domain.AdminRolePermissions;
using Microsoft.EntityFrameworkCore;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Commands;

public sealed class AdminRolePermissionCommandRepository : IAdminRolePermissionCommandRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminRolePermissionCommandRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AdminRolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await _dbContext.AdminRolePermissions
            .Where(x => x.AdminRoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyCollection<AdminRolePermission> adminRolePermissions, CancellationToken cancellationToken)
    {
        await _dbContext.AdminRolePermissions.AddRangeAsync(adminRolePermissions, cancellationToken);
    }

    public void RemoveRange(IReadOnlyCollection<AdminRolePermission> adminRolePermissions)
    {
        _dbContext.AdminRolePermissions.RemoveRange(adminRolePermissions);
    }

    public Task<bool> ExistsByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminRolePermissions.AnyAsync(x => x.AdminPermissionId == permissionId, cancellationToken);
    }
}

