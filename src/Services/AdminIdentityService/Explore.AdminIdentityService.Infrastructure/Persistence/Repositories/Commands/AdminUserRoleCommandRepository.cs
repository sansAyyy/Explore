using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Domain.AdminUserRoles;
using Microsoft.EntityFrameworkCore;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Commands;

public sealed class AdminUserRoleCommandRepository : IAdminUserRoleCommandRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminUserRoleCommandRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AdminUserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.AdminUserRoles
            .Where(x => x.AdminUserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyCollection<AdminUserRole> adminUserRoles, CancellationToken cancellationToken)
    {
        await _dbContext.AdminUserRoles.AddRangeAsync(adminUserRoles, cancellationToken);
    }

    public void RemoveRange(IReadOnlyCollection<AdminUserRole> adminUserRoles)
    {
        _dbContext.AdminUserRoles.RemoveRange(adminUserRoles);
    }

    public Task<bool> ExistsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUserRoles.AnyAsync(x => x.AdminRoleId == roleId, cancellationToken);
    }
}

