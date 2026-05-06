using Explore.MessageCenterService.Application.Abstractions.Persistence;

namespace Explore.MessageCenterService.Infrastructure.Persistence;

public sealed class MessageCenterUnitOfWork : IMessageCenterUnitOfWork
{
    private readonly MessageCenterDbContext _dbContext;

    public MessageCenterUnitOfWork(MessageCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

