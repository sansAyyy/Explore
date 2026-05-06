using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Domain.SiteMessages;
using Microsoft.EntityFrameworkCore;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Repositories.Commands;

public sealed class SiteMessageCommandRepository : ISiteMessageCommandRepository, IScopeDependency
{
    private readonly MessageCenterDbContext _dbContext;

    public SiteMessageCommandRepository(MessageCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SiteMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.SiteMessages.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SiteMessage>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.SiteMessages
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(SiteMessage siteMessage, CancellationToken cancellationToken)
    {
        return _dbContext.SiteMessages.AddAsync(siteMessage, cancellationToken).AsTask();
    }
}

