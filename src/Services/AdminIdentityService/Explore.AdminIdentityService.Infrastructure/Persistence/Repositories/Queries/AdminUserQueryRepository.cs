using BuildingBlocks.Common.Pagination;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Queries;

public sealed class AdminUserQueryRepository : IAdminUserQueryRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminUserQueryRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminUserDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(MapDetail())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminUserBasicResponse>> GetPagedAsync(
        GetPagedAdminUsersRequest request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AdminUsers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.UserName, $"%{keyword}%") ||
                EF.Functions.ILike(x.Email, $"%{keyword}%") ||
                (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, $"%{keyword}%")) ||
                EF.Functions.ILike(x.DisplayName, $"%{keyword}%"));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.UserName)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapBasic())
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminUserBasicResponse>(totalCount, items);
    }

    private static Expression<Func<Domain.AdminUsers.AdminUser, AdminUserBasicResponse>> MapBasic()
    {
        return adminUser => new AdminUserBasicResponse(
            adminUser.Id,
            adminUser.UserName,
            adminUser.Email,
            adminUser.PhoneNumber,
            adminUser.DisplayName,
            adminUser.IsActive,
            adminUser.CreatedAt,
            adminUser.UpdatedAt,
            adminUser.LastLoginAt);
    }

    private static Expression<Func<Domain.AdminUsers.AdminUser, AdminUserDetailResponse>> MapDetail()
    {
        return adminUser => new AdminUserDetailResponse(
            adminUser.Id,
            adminUser.UserName,
            adminUser.Email,
            adminUser.PhoneNumber,
            adminUser.DisplayName,
            adminUser.IsActive,
            adminUser.CreatedAt,
            adminUser.CreatedBy,
            adminUser.UpdatedAt,
            adminUser.UpdatedBy,
            adminUser.LastLoginAt,
            adminUser.Version);
    }
}

