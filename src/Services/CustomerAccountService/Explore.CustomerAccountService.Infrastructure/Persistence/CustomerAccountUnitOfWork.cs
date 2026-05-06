using Explore.CustomerAccountService.Application.Abstractions.Persistence;

namespace Explore.CustomerAccountService.Infrastructure.Persistence;

public sealed class CustomerAccountUnitOfWork : ICustomerAccountUnitOfWork
{
    private readonly CustomerAccountDbContext _dbContext;

    public CustomerAccountUnitOfWork(CustomerAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

