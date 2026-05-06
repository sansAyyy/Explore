using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Domain.AdminUsers;
using Microsoft.EntityFrameworkCore;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Repositories.Commands;

public sealed class AdminUserCommandRepository : IAdminUserCommandRepository, IScopeDependency
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminUserCommandRepository(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<AdminUser?> GetByAccountAsync(string account, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers.SingleOrDefaultAsync(
            x => x.UserName == account || x.Email == account,
            cancellationToken);
    }

    public Task<AdminUser?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers.SingleOrDefaultAsync(
            x => x.PhoneNumber == phoneNumber,
            cancellationToken);
    }

    public Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers.AddAsync(adminUser, cancellationToken).AsTask();
    }

    public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers.AnyAsync(
            x => x.UserName == userName && (!excludedId.HasValue || x.Id != excludedId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers.AnyAsync(
            x => x.Email == email && (!excludedId.HasValue || x.Id != excludedId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, Guid? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.AdminUsers.AnyAsync(
            x => x.PhoneNumber == phoneNumber && (!excludedId.HasValue || x.Id != excludedId.Value),
            cancellationToken);
    }

    public void Remove(AdminUser adminUser)
    {
        _dbContext.AdminUsers.Remove(adminUser);
    }
}

