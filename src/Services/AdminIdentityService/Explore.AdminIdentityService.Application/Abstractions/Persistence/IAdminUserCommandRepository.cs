using Explore.AdminIdentityService.Domain.AdminUsers;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminUserCommandRepository
{
    Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AdminUser?> GetByAccountAsync(string account, CancellationToken cancellationToken);

    Task<AdminUser?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);

    Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken);

    Task<bool> ExistsByUserNameAsync(string userName, Guid? excludedId, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, Guid? excludedId, CancellationToken cancellationToken);

    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, Guid? excludedId, CancellationToken cancellationToken);

    void Remove(AdminUser adminUser);
}

