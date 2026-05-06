namespace BuildingBlocks.DistributedLocking.Redis.Internal;

internal interface IRedisDistributedLockStore
{
    Task<bool> TryAcquireAsync(
        string resource,
        string ownerToken,
        TimeSpan leaseTtl,
        CancellationToken cancellationToken);

    Task<bool> ReleaseAsync(
        string resource,
        string ownerToken,
        CancellationToken cancellationToken);

    Task<bool> RenewAsync(
        string resource,
        string ownerToken,
        TimeSpan leaseTtl,
        CancellationToken cancellationToken);
}

