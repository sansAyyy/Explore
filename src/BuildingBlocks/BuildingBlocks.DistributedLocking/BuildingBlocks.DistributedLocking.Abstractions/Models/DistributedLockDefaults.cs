namespace BuildingBlocks.DistributedLocking.Models;

public sealed class DistributedLockDefaults
{
    public static readonly TimeSpan DefaultLeaseTtl = TimeSpan.FromSeconds(30);

    public static readonly TimeSpan DefaultWaitTimeout = TimeSpan.Zero;

    public static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromMilliseconds(200);

    public TimeSpan LeaseTtl { get; init; } = DefaultLeaseTtl;

    public TimeSpan WaitTimeout { get; init; } = DefaultWaitTimeout;

    public TimeSpan RetryInterval { get; init; } = DefaultRetryInterval;

    public bool AutoRenew { get; init; }
}

