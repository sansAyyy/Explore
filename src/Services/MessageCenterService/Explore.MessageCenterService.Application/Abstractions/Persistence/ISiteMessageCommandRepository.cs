using Explore.MessageCenterService.Domain.SiteMessages;

namespace Explore.MessageCenterService.Application.Abstractions.Persistence;

public interface ISiteMessageCommandRepository
{
    Task<SiteMessage?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SiteMessage>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(SiteMessage siteMessage, CancellationToken cancellationToken);
}

