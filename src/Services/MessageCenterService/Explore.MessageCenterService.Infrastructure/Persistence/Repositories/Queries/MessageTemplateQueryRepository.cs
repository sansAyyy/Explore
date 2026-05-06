using BuildingBlocks.Common.Pagination;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Responses;
using Microsoft.EntityFrameworkCore;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Repositories.Queries;

public sealed class MessageTemplateQueryRepository : IMessageTemplateQueryRepository, IScopeDependency
{
    private readonly MessageCenterDbContext _dbContext;

    public MessageTemplateQueryRepository(MessageCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<MessageTemplateBasicResponse>> GetPagedAsync(
        GetPagedMessageTemplatesRequest request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.MessageTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x => x.Code.Contains(keyword) || x.Name.Contains(keyword));
        }

        if (request.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == request.IsEnabled.Value);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Code)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new MessageTemplateBasicResponse(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsEnabled,
                x.ChannelType))
            .ToListAsync(cancellationToken);

        return new PagedResult<MessageTemplateBasicResponse>(totalCount, items);
    }

    public async Task<MessageTemplateDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.MessageTemplates
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new MessageTemplateDetailResponse(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsEnabled,
                x.ChannelType,
                x.TitleTemplate,
                x.BodyTemplate))
            .SingleOrDefaultAsync(cancellationToken);
    }
}

