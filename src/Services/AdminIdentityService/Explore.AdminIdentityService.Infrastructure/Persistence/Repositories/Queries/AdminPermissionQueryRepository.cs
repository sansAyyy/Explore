using BuildingBlocks.Common.Pagination;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Queries;

public sealed class AdminPermissionQueryRepository : IAdminPermissionQueryRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminPermissionQueryRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AdminPermissionBasicResponse>> GetRootsAsync(bool? isActive, CancellationToken cancellationToken)
    {
        var query = _dbContext.AdminPermissions
            .AsNoTracking()
            .Where(x => x.ParentId == null);

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(x => x.Code)
            .Select(MapBasic())
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminPermissionTreeNodeResponse?> GetDescendantsAsync(Guid rootId, bool? isActive, CancellationToken cancellationToken)
    {
        var root = await _dbContext.AdminPermissions
            .AsNoTracking()
            .Where(x => x.Id == rootId)
            .Select(MapBasic())
            .SingleOrDefaultAsync(cancellationToken);

        if (root is null || root.ParentId is not null)
        {
            return null;
        }

        var permissions = await _dbContext.AdminPermissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(MapBasic())
            .ToListAsync(cancellationToken);

        var byParentId = permissions
            .Where(x => x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());

        AdminPermissionTreeNodeResponse BuildNode(AdminPermissionBasicResponse permission)
        {
            var children = byParentId.TryGetValue(permission.Id, out var childPermissions)
                ? childPermissions
                    .Where(child => !isActive.HasValue || child.IsActive == isActive.Value)
                    .Select(BuildNode)
                    .ToArray()
                : [];

            return new AdminPermissionTreeNodeResponse(
                permission.Id,
                permission.ParentId,
                permission.Code,
                permission.Name,
                permission.Description,
                permission.ResourceType,
                permission.IsActive,
                children);
        }

        return BuildNode(root);
    }

    public Task<AdminPermissionDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.AdminPermissions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(MapDetail())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminPermissionBasicResponse>> GetPagedAsync(GetPagedAdminPermissionsRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AdminPermissions.AsNoTracking();

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

        if (request.ResourceType.HasValue)
        {
            query = query.Where(x => x.ResourceType == request.ResourceType.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Code)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapBasic())
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminPermissionBasicResponse>(totalCount, items);
    }

    private static Expression<Func<Domain.AdminPermissions.AdminPermission, AdminPermissionBasicResponse>> MapBasic()
    {
        return permission => new AdminPermissionBasicResponse(
            permission.Id,
            permission.ParentId,
            permission.Code,
            permission.Name,
            permission.Description,
            permission.ResourceType,
            permission.IsActive,
            permission.CreatedAt,
            permission.UpdatedAt);
    }

    private static Expression<Func<Domain.AdminPermissions.AdminPermission, AdminPermissionDetailResponse>> MapDetail()
    {
        return permission => new AdminPermissionDetailResponse(
            permission.Id,
            permission.ParentId,
            permission.Code,
            permission.Name,
            permission.Description,
            permission.ResourceType,
            permission.IsActive,
            permission.CreatedAt,
            permission.CreatedBy,
            permission.UpdatedAt,
            permission.UpdatedBy,
            permission.Version);
    }
}

