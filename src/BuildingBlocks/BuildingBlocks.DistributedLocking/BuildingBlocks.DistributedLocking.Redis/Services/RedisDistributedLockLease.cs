using BuildingBlocks.DistributedLocking.Abstractions;
using BuildingBlocks.DistributedLocking.Redis.Internal;

namespace BuildingBlocks.DistributedLocking.Redis.Services;

public sealed class RedisDistributedLockLease : IDistributedLockLease
{
    private readonly IRedisDistributedLockStore _store;
    private readonly CancellationTokenSource? _renewalCancellationTokenSource;
    private readonly Task? _renewalTask;

    private int _releaseState;

    internal RedisDistributedLockLease(
        string resource,
        string ownerToken,
        DateTime acquiredAtUtc,
        TimeSpan leaseTtl,
        IRedisDistributedLockStore store,
        bool autoRenew,
        TimeSpan autoRenewInterval)
    {
        Resource = resource;
        OwnerToken = ownerToken;
        AcquiredAtUtc = acquiredAtUtc;
        LeaseTtl = leaseTtl;
        _store = store;

        if (autoRenew)
        {
            _renewalCancellationTokenSource = new CancellationTokenSource();
            _renewalTask = RunRenewalLoopAsync(autoRenewInterval, _renewalCancellationTokenSource.Token);
        }
    }

    public string Resource { get; }

    public string OwnerToken { get; }

    public DateTime AcquiredAtUtc { get; }

    public TimeSpan LeaseTtl { get; }

    public async Task<bool> ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _releaseState, 1) != 0)
        {
            return false;
        }

        if (_renewalCancellationTokenSource is not null)
        {
            await _renewalCancellationTokenSource.CancelAsync();
        }

        if (_renewalTask is not null)
        {
            try
            {
                await _renewalTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        return await _store.ReleaseAsync(Resource, OwnerToken, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(ReleaseAsync());
    }

    private async Task RunRenewalLoopAsync(TimeSpan autoRenewInterval, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(autoRenewInterval, cancellationToken);
                if (Volatile.Read(ref _releaseState) != 0)
                {
                    break;
                }

                var renewed = await _store.RenewAsync(Resource, OwnerToken, LeaseTtl, cancellationToken);
                if (!renewed)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch
        {
            // Renewal failures stop background extension and allow the lease to expire naturally.
        }
    }
}

