using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;
using Explore.CustomerAccountService.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Explore.CustomerAccountService.Infrastructure.Persistence.Repositories;

public sealed class CustomerCommandRepository : ICustomerCommandRepository, IScopeDependency
{
    private readonly CustomerAccountDbContext _dbContext;

    public CustomerCommandRepository(CustomerAccountDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Customers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        return _dbContext.Customers.SingleOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        return _dbContext.Customers.AddAsync(customer, cancellationToken).AsTask();
    }

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.Customers.AnyAsync(
            x => x.Email == email && (!excludedId.HasValue || x.Id != excludedId.Value),
            cancellationToken);
    }
}

