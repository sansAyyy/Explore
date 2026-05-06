using Explore.MessageCenterService.Domain.NotificationDispatches;

namespace Explore.MessageCenterService.Application.Abstractions.Persistence;

public interface INotificationDispatchCommandRepository
{
    Task AddRangeAsync(IEnumerable<NotificationDispatch> dispatches, CancellationToken cancellationToken);
}

