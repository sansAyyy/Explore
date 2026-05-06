namespace BuildingBlocks.DistributedLocking.Abstractions;

public interface IDistributedLockLease : IAsyncDisposable
{
    string Resource { get; }

    string OwnerToken { get; }

    DateTime AcquiredAtUtc { get; }

    TimeSpan LeaseTtl { get; }

    Task<bool> ReleaseAsync(CancellationToken cancellationToken = default);
}

