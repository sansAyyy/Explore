using Explore.AdminIdentityService.Application.Abstractions.Persistence;

namespace Explore.AdminIdentityService.Infrastructure.Persistence;

public sealed class AdminIdentityUnitOfWork : IAdminIdentityUnitOfWork
{
    private readonly AdminIdentityDbContext _dbContext;

    public AdminIdentityUnitOfWork(AdminIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

