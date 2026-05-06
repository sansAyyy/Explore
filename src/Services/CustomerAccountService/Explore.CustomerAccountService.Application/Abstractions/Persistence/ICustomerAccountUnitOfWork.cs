namespace Explore.CustomerAccountService.Application.Abstractions.Persistence;

public interface ICustomerAccountUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
}

