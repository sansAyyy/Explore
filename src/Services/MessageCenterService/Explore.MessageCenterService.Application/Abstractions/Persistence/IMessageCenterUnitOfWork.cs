namespace Explore.MessageCenterService.Application.Abstractions.Persistence;

public interface IMessageCenterUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
}

