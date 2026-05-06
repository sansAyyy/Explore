namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminIdentityUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
}

