using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Responses;
using Microsoft.EntityFrameworkCore;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Queries;

public sealed class AdminUserRoleQueryRepository : IAdminUserRoleQueryRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminUserRoleQueryRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminUserRolesResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.AdminUsers.AsNoTracking().AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            return null;
        }

        var roles = await (
                from userRole in _dbContext.AdminUserRoles.AsNoTracking()
                join role in _dbContext.AdminRoles.AsNoTracking()
                    on userRole.AdminRoleId equals role.Id
                where userRole.AdminUserId == userId
                orderby role.Code
                select new AssignedUserRoleResponse(role.Id, role.Code, role.Name, role.IsActive))
            .ToListAsync(cancellationToken);

        return new AdminUserRolesResponse(userId, roles);
    }
}

