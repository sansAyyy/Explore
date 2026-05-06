using Explore.CustomerAccountService.Domain.Customers;

namespace Explore.CustomerAccountService.Application.Abstractions.Persistence;

public interface ICustomerCommandRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);

    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId, CancellationToken cancellationToken);
}

