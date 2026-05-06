using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Domain.NotificationDispatches;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Repositories.Commands;

public sealed class NotificationDispatchCommandRepository : INotificationDispatchCommandRepository, IScopeDependency
{
    private readonly MessageCenterDbContext _dbContext;

    public NotificationDispatchCommandRepository(MessageCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddRangeAsync(IEnumerable<NotificationDispatch> dispatches, CancellationToken cancellationToken)
    {
        return _dbContext.NotificationDispatches.AddRangeAsync(dispatches, cancellationToken);
    }
}

