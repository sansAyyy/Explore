using BuildingBlocks.Common.Pagination;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Responses;
using Explore.AdminIdentityService.Domain.AdminPermissions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Queries;

public sealed class AdminRoleQueryRepository : IAdminRoleQueryRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminRoleQueryRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminRoleDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.AdminRoles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(MapDetail())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminRoleBasicResponse>> GetPagedAsync(GetPagedAdminRolesRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AdminRoles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Code, $"%{keyword}%") ||
                EF.Functions.ILike(x.Name, $"%{keyword}%") ||
                (x.Description != null && EF.Functions.ILike(x.Description, $"%{keyword}%")));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Code)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapBasic())
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminRoleBasicResponse>(totalCount, items);
    }

    public async Task<AdminRolePermissionsResponse?> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var roleExists = await _dbContext.AdminRoles.AsNoTracking().AnyAsync(x => x.Id == roleId, cancellationToken);
        if (!roleExists)
        {
            return null;
        }

        var permissions = await (
                from rolePermission in _dbContext.AdminRolePermissions.AsNoTracking()
                join permission in _dbContext.AdminPermissions.AsNoTracking()
                    on rolePermission.AdminPermissionId equals permission.Id
                where rolePermission.AdminRoleId == roleId
                orderby permission.Code
                select new AssignedRolePermissionResponse(
                    permission.Id,
                    permission.Code,
                    permission.Name,
                    permission.ResourceType,
                    permission.IsActive))
            .ToListAsync(cancellationToken);

        return new AdminRolePermissionsResponse(roleId, permissions);
    }

    private static Expression<Func<Domain.AdminRoles.AdminRole, AdminRoleBasicResponse>> MapBasic()
    {
        return role => new AdminRoleBasicResponse(
            role.Id,
            role.Code,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt);
    }

    private static Expression<Func<Domain.AdminRoles.AdminRole, AdminRoleDetailResponse>> MapDetail()
    {
        return role => new AdminRoleDetailResponse(
            role.Id,
            role.Code,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.CreatedBy,
            role.UpdatedAt,
            role.UpdatedBy,
            role.Version);
    }
}

