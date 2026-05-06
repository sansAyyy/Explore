using BuildingBlocks.DistributedLocking.Abstractions;
using BuildingBlocks.DistributedLocking.Models;
using BuildingBlocks.DistributedLocking.Redis.Internal;

namespace BuildingBlocks.DistributedLocking.Redis.Services;

public sealed class RedisDistributedLockService : IDistributedLockService
{
    private readonly IRedisDistributedLockStore _store;
    private readonly DistributedLockDefaults _defaults;

    internal RedisDistributedLockService(IRedisDistributedLockStore store)
        : this(store, new DistributedLockDefaults())
    {
    }

    internal RedisDistributedLockService(IRedisDistributedLockStore store, DistributedLockDefaults defaults)
    {
        _store = store;
        _defaults = defaults;
    }

    public Task<IDistributedLockLease?> TryAcquireAsync(
        string resource,
        CancellationToken cancellationToken = default)
    {
        return TryAcquireAsync(resource, CreateDefaultOptions(), cancellationToken);
    }

    public async Task<IDistributedLockLease?> TryAcquireAsync(
        string resource,
        DistributedLockAcquireOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        ArgumentNullException.ThrowIfNull(options);

        var effectiveOptions = CreateEffectiveOptions(options);
        var ownerToken = Guid.NewGuid().ToString("N");
        DateTime? deadlineUtc = effectiveOptions.WaitTimeout == TimeSpan.Zero
            ? null
            : DateTime.UtcNow.Add(effectiveOptions.WaitTimeout);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await _store.TryAcquireAsync(resource, ownerToken, effectiveOptions.LeaseTtl, cancellationToken))
            {
                return new RedisDistributedLockLease(
                    resource,
                    ownerToken,
                    DateTime.UtcNow,
                    effectiveOptions.LeaseTtl,
                    _store,
                    effectiveOptions.AutoRenew,
                    effectiveOptions.AutoRenewInterval);
            }

            if (deadlineUtc is null)
            {
                return null;
            }

            var remaining = deadlineUtc.Value - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                return null;
            }

            var delay = remaining < effectiveOptions.RetryInterval
                ? remaining
                : effectiveOptions.RetryInterval;

            await Task.Delay(delay, cancellationToken);
        }
    }

    private DistributedLockAcquireOptions CreateDefaultOptions()
    {
        return new DistributedLockAcquireOptions
        {
            LeaseTtl = _defaults.LeaseTtl,
            WaitTimeout = _defaults.WaitTimeout,
            RetryInterval = _defaults.RetryInterval,
            AutoRenew = _defaults.AutoRenew
        };
    }

    private EffectiveDistributedLockAcquireOptions CreateEffectiveOptions(DistributedLockAcquireOptions options)
    {
        if (options.LeaseTtl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options.LeaseTtl), "LeaseTtl must be greater than zero.");
        }

        if (options.WaitTimeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options.WaitTimeout), "WaitTimeout cannot be negative.");
        }

        var retryInterval = options.RetryInterval <= TimeSpan.Zero
            ? _defaults.RetryInterval
            : options.RetryInterval;

        var autoRenewInterval = ResolveAutoRenewInterval(options.AutoRenewInterval, options.LeaseTtl);

        return new EffectiveDistributedLockAcquireOptions(
            options.LeaseTtl,
            options.WaitTimeout,
            retryInterval,
            options.AutoRenew,
            autoRenewInterval);
    }

    private static TimeSpan ResolveAutoRenewInterval(TimeSpan? configuredInterval, TimeSpan leaseTtl)
    {
        // When the configured interval is unusable, renew well before half of the TTL so
        // background scheduling jitter does not let the lease expire unexpectedly.
        var fallbackInterval = TimeSpan.FromMilliseconds(Math.Max(1, Math.Floor(leaseTtl.TotalMilliseconds / 3)));
        if (!configuredInterval.HasValue ||
            configuredInterval.Value <= TimeSpan.Zero ||
            configuredInterval.Value >= leaseTtl)
        {
            return fallbackInterval;
        }

        return configuredInterval.Value;
    }

    private sealed record EffectiveDistributedLockAcquireOptions(
        TimeSpan LeaseTtl,
        TimeSpan WaitTimeout,
        TimeSpan RetryInterval,
        bool AutoRenew,
        TimeSpan AutoRenewInterval);
}

