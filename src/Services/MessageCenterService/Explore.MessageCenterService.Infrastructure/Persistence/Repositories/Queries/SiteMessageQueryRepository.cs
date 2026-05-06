using BuildingBlocks.Common.Pagination;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;
using Microsoft.EntityFrameworkCore;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Repositories.Queries;

public sealed class SiteMessageQueryRepository : ISiteMessageQueryRepository, IScopeDependency
{
    private readonly MessageCenterDbContext _dbContext;

    public SiteMessageQueryRepository(MessageCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SiteMessageBasicResponse>> GetPagedAsync(
        Guid userId,
        GetPagedSiteMessagesRequest request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.SiteMessages
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (request.IsRead.HasValue)
        {
            query = query.Where(x => x.IsRead == request.IsRead.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new SiteMessageBasicResponse(
                x.Id,
                x.UserId,
                x.Title,
                x.Content.Length > 100 ? x.Content.Substring(0, 100) : x.Content,
                x.IsRead,
                x.CreatedAt,
                x.ReadAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<SiteMessageBasicResponse>(totalCount, items);
    }

    public Task<SiteMessageDetailResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.SiteMessages
            .AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new SiteMessageDetailResponse(
                x.Id,
                x.DispatchId,
                x.UserId,
                x.Title,
                x.Content,
                x.IsRead,
                x.CreatedAt,
                x.ReadAt))
            .SingleOrDefaultAsync(cancellationToken);
    }
}

