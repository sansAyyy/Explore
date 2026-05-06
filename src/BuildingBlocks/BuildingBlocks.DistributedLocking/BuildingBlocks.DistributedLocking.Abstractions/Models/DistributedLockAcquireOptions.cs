namespace BuildingBlocks.DistributedLocking.Models;

public sealed class DistributedLockAcquireOptions
{
    public TimeSpan LeaseTtl { get; set; } = DistributedLockDefaults.DefaultLeaseTtl;

    public TimeSpan WaitTimeout { get; set; } = DistributedLockDefaults.DefaultWaitTimeout;

    public TimeSpan RetryInterval { get; set; } = DistributedLockDefaults.DefaultRetryInterval;

    public bool AutoRenew { get; set; }

    public TimeSpan? AutoRenewInterval { get; set; }
}

