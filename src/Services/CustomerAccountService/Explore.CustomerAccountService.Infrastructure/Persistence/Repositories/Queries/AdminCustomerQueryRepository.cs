using BuildingBlocks.Common.Pagination;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Explore.CustomerAccountService.Infrastructure.Persistence.Repositories.Queries;

public sealed class AdminCustomerQueryRepository : IAdminCustomerQueryRepository, IScopeDependency
{
    private readonly CustomerAccountDbContext _dbContext;

    public AdminCustomerQueryRepository(CustomerAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminCustomerDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(MapDetail())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminCustomerBasicResponse>> GetPagedAsync(
        GetPagedAdminCustomersRequest request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.PhoneNumber, $"%{keyword}%") ||
                EF.Functions.ILike(x.NickName, $"%{keyword}%") ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{keyword}%")));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapBasic())
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminCustomerBasicResponse>(totalCount, items);
    }

    private static Expression<Func<Domain.Customers.Customer, AdminCustomerBasicResponse>> MapBasic()
    {
        return customer => new AdminCustomerBasicResponse(
            customer.Id,
            customer.PhoneNumber,
            customer.NickName,
            customer.AvatarUrl,
            customer.Email,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt,
            customer.LastLoginAt);
    }

    private static Expression<Func<Domain.Customers.Customer, AdminCustomerDetailResponse>> MapDetail()
    {
        return customer => new AdminCustomerDetailResponse(
            customer.Id,
            customer.PhoneNumber,
            customer.NickName,
            customer.AvatarUrl,
            customer.Email,
            customer.IsActive,
            customer.CreatedAt,
            customer.CreatedBy,
            customer.UpdatedAt,
            customer.UpdatedBy,
            customer.LastLoginAt,
            customer.Version);
    }
}

