using BuildingBlocks.DistributedLocking.Models;

namespace BuildingBlocks.DistributedLocking.Abstractions;

public interface IDistributedLockService
{
    Task<IDistributedLockLease?> TryAcquireAsync(
        string resource,
        CancellationToken cancellationToken = default);

    Task<IDistributedLockLease?> TryAcquireAsync(
        string resource,
        DistributedLockAcquireOptions options,
        CancellationToken cancellationToken = default);
}

