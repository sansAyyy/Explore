using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Authorization.Dtos.Responses;
using Explore.AdminIdentityService.Domain.AdminPermissions;
using Microsoft.EntityFrameworkCore;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Queries;

public sealed class AdminAuthorizationQueryRepository : IAdminAuthorizationQueryRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminAuthorizationQueryRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurrentAdminAuthorizationResponse?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.AdminUsers
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.DisplayName
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleCodes = await (
                from userRole in _dbContext.AdminUserRoles.AsNoTracking()
                join role in _dbContext.AdminRoles.AsNoTracking()
                    on userRole.AdminRoleId equals role.Id
                where userRole.AdminUserId == userId && role.IsActive
                orderby role.Code
                select role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await (
                from userRole in _dbContext.AdminUserRoles.AsNoTracking()
                join role in _dbContext.AdminRoles.AsNoTracking()
                    on userRole.AdminRoleId equals role.Id
                join rolePermission in _dbContext.AdminRolePermissions.AsNoTracking()
                    on role.Id equals rolePermission.AdminRoleId
                join permission in _dbContext.AdminPermissions.AsNoTracking()
                    on rolePermission.AdminPermissionId equals permission.Id
                where userRole.AdminUserId == userId &&
                      role.IsActive &&
                      permission.IsActive
                select new
                {
                    permission.Code,
                    permission.ResourceType
                })
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissionCodes = permissions
            .Select(x => x.Code)
            .OrderBy(x => x)
            .ToArray();

        var pagePermissionCodes = permissions
            .Where(x => x.ResourceType == PermissionResourceType.Page)
            .Select(x => x.Code)
            .OrderBy(x => x)
            .ToArray();

        var buttonPermissionCodes = permissions
            .Where(x => x.ResourceType == PermissionResourceType.Button)
            .Select(x => x.Code)
            .OrderBy(x => x)
            .ToArray();

        return new CurrentAdminAuthorizationResponse(
            user.Id,
            user.UserName,
            user.DisplayName,
            roleCodes,
            permissionCodes,
            pagePermissionCodes,
            buttonPermissionCodes);
    }
}

