using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.DistributedLocking.Models;
using BuildingBlocks.DistributedLocking.Redis.Internal;
using BuildingBlocks.DistributedLocking.Redis.Services;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class DistributedLockServiceTests
{
    [Fact]
    public async Task TryAcquireAsync_ShouldReturnLease_WhenResourceIsAvailable()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);

        var lease = await service.TryAcquireAsync("locks:test:available", CancellationToken.None);

        Assert.NotNull(lease);
        Assert.Equal("locks:test:available", lease.Resource);
        Assert.False(string.IsNullOrWhiteSpace(lease.OwnerToken));
        Assert.True(store.Exists("locks:test:available"));
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldReturnNull_WhenResourceIsAlreadyLocked()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        await using var firstLease = await service.TryAcquireAsync("locks:test:busy", CancellationToken.None);

        var secondLease = await service.TryAcquireAsync("locks:test:busy", CancellationToken.None);

        Assert.NotNull(firstLease);
        Assert.Null(secondLease);
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldAllowReacquire_AfterRelease()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        var firstLease = await service.TryAcquireAsync("locks:test:reacquire", CancellationToken.None);

        Assert.NotNull(firstLease);
        Assert.True(await firstLease.ReleaseAsync(CancellationToken.None));

        await using var secondLease = await service.TryAcquireAsync("locks:test:reacquire", CancellationToken.None);

        Assert.NotNull(secondLease);
    }

    [Fact]
    public async Task ReleaseAsync_ShouldFail_WhenLeaseOwnerDoesNotMatch()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        await using var firstLease = await service.TryAcquireAsync("locks:test:owner", CancellationToken.None);
        var rogueLease = new RedisDistributedLockLease(
            "locks:test:owner",
            "rogue-owner",
            DateTime.UtcNow,
            TimeSpan.FromSeconds(5),
            store,
            autoRenew: false,
            autoRenewInterval: TimeSpan.FromSeconds(1));

        var released = await rogueLease.ReleaseAsync(CancellationToken.None);

        Assert.NotNull(firstLease);
        Assert.False(released);
        Assert.True(store.Exists("locks:test:owner"));
    }

    [Fact]
    public async Task DisposeAsync_ShouldReleaseHeldLock()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        var lease = await service.TryAcquireAsync("locks:test:dispose", CancellationToken.None);

        Assert.NotNull(lease);
        await lease.DisposeAsync();

        Assert.False(store.Exists("locks:test:dispose"));
    }

    [Fact]
    public async Task ReleaseAsync_ShouldBeIdempotent()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        var lease = await service.TryAcquireAsync("locks:test:idempotent", CancellationToken.None);

        Assert.NotNull(lease);
        var firstRelease = await lease.ReleaseAsync(CancellationToken.None);
        var secondRelease = await lease.ReleaseAsync(CancellationToken.None);

        Assert.True(firstRelease);
        Assert.False(secondRelease);
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldAttemptOnce_WhenWaitTimeoutIsZero()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        await using var firstLease = await service.TryAcquireAsync("locks:test:no-wait", CancellationToken.None);
        store.ResetTryAcquireCount("locks:test:no-wait");

        var secondLease = await service.TryAcquireAsync(
            "locks:test:no-wait",
            new DistributedLockAcquireOptions
            {
                LeaseTtl = TimeSpan.FromSeconds(5),
                WaitTimeout = TimeSpan.Zero,
                RetryInterval = TimeSpan.FromMilliseconds(10)
            },
            CancellationToken.None);

        Assert.NotNull(firstLease);
        Assert.Null(secondLease);
        Assert.Equal(1, store.GetTryAcquireCount("locks:test:no-wait"));
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldRetryUntilLockBecomesAvailable()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        var firstLease = await service.TryAcquireAsync("locks:test:retry", CancellationToken.None);

        Assert.NotNull(firstLease);

        var acquireTask = service.TryAcquireAsync(
            "locks:test:retry",
            new DistributedLockAcquireOptions
            {
                LeaseTtl = TimeSpan.FromSeconds(5),
                WaitTimeout = TimeSpan.FromSeconds(2),
                RetryInterval = TimeSpan.FromMilliseconds(25)
            },
            CancellationToken.None);

        await Task.Delay(80);
        await firstLease.ReleaseAsync(CancellationToken.None);
        await using var secondLease = await acquireTask;

        Assert.NotNull(secondLease);
        Assert.True(store.GetTryAcquireCount("locks:test:retry") > 1);
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldThrow_WhenWaitingIsCancelled()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        await using var firstLease = await service.TryAcquireAsync("locks:test:cancel", CancellationToken.None);
        using var cancellationTokenSource = new CancellationTokenSource();

        var acquireTask = service.TryAcquireAsync(
            "locks:test:cancel",
            new DistributedLockAcquireOptions
            {
                LeaseTtl = TimeSpan.FromSeconds(5),
                WaitTimeout = TimeSpan.FromSeconds(5),
                RetryInterval = TimeSpan.FromMilliseconds(50)
            },
            cancellationTokenSource.Token);

        cancellationTokenSource.CancelAfter(80);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await acquireTask);
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldAllowLockToExpire_WhenAutoRenewIsDisabled()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        var lease = await service.TryAcquireAsync(
            "locks:test:no-renew",
            new DistributedLockAcquireOptions
            {
                LeaseTtl = TimeSpan.FromMilliseconds(80),
                AutoRenew = false
            },
            CancellationToken.None);

        Assert.NotNull(lease);
        await Task.Delay(140);
        await using var secondLease = await service.TryAcquireAsync("locks:test:no-renew", CancellationToken.None);

        Assert.NotNull(secondLease);
        Assert.Equal(0, store.GetRenewCount("locks:test:no-renew"));
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldKeepLockAlive_WhenAutoRenewIsEnabled()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        var lease = await service.TryAcquireAsync(
            "locks:test:auto-renew",
            new DistributedLockAcquireOptions
            {
                LeaseTtl = TimeSpan.FromMilliseconds(120),
                AutoRenew = true,
                AutoRenewInterval = TimeSpan.FromMilliseconds(30)
            },
            CancellationToken.None);

        Assert.NotNull(lease);
        await Task.Delay(260);
        var blockedLease = await service.TryAcquireAsync("locks:test:auto-renew", CancellationToken.None);

        Assert.Null(blockedLease);
        Assert.True(store.GetRenewCount("locks:test:auto-renew") > 0);

        await lease.DisposeAsync();
        await using var secondLease = await service.TryAcquireAsync("locks:test:auto-renew", CancellationToken.None);

        Assert.NotNull(secondLease);
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldFallbackAutoRenewInterval_WhenConfiguredIntervalIsInvalid()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);
        var lease = await service.TryAcquireAsync(
            "locks:test:renew-fallback",
            new DistributedLockAcquireOptions
            {
                LeaseTtl = TimeSpan.FromMilliseconds(120),
                AutoRenew = true,
                AutoRenewInterval = TimeSpan.FromMilliseconds(120)
            },
            CancellationToken.None);

        Assert.NotNull(lease);
        await Task.Delay(80);
        Assert.True(store.GetRenewCount("locks:test:renew-fallback") > 0);
        var blockedLease = await service.TryAcquireAsync("locks:test:renew-fallback", CancellationToken.None);

        Assert.Null(blockedLease);
        Assert.True(store.GetRenewCount("locks:test:renew-fallback") > 0);

        await lease.DisposeAsync();
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldThrow_WhenResourceIsBlank()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.TryAcquireAsync("   ", CancellationToken.None));
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldThrow_WhenLeaseTtlIsInvalid()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.TryAcquireAsync(
                "locks:test:invalid-ttl",
                new DistributedLockAcquireOptions
                {
                    LeaseTtl = TimeSpan.FromMilliseconds(-1)
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldThrow_WhenWaitTimeoutIsNegative()
    {
        var store = new FakeRedisDistributedLockStore();
        var service = CreateService(store);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await service.TryAcquireAsync(
                "locks:test:invalid-wait",
                new DistributedLockAcquireOptions
                {
                    WaitTimeout = TimeSpan.FromMilliseconds(-1)
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task TryAcquireAsync_ShouldFallbackRetryInterval_WhenNonPositiveIntervalIsProvided()
    {
        var store = new FakeRedisDistributedLockStore();
        var defaults = new DistributedLockDefaults
        {
            RetryInterval = TimeSpan.FromMilliseconds(20)
        };
        var service = CreateService(store, defaults);
        var firstLease = await service.TryAcquireAsync("locks:test:retry-fallback", CancellationToken.None);

        Assert.NotNull(firstLease);

        var acquireTask = service.TryAcquireAsync(
            "locks:test:retry-fallback",
            new DistributedLockAcquireOptions
            {
                LeaseTtl = TimeSpan.FromSeconds(5),
                WaitTimeout = TimeSpan.FromMilliseconds(120),
                RetryInterval = TimeSpan.Zero
            },
            CancellationToken.None);

        await Task.Delay(45);
        await firstLease.ReleaseAsync(CancellationToken.None);
        await using var secondLease = await acquireTask;

        Assert.NotNull(secondLease);
        Assert.True(store.GetTryAcquireCount("locks:test:retry-fallback") > 2);
    }

    private static RedisDistributedLockService CreateService(
        FakeRedisDistributedLockStore store,
        DistributedLockDefaults? defaults = null)
    {
        return defaults is null
            ? new RedisDistributedLockService(store)
            : new RedisDistributedLockService(store, defaults);
    }

    private sealed class FakeRedisDistributedLockStore : IRedisDistributedLockStore
    {
        private readonly Lock _sync = new();
        private readonly Dictionary<string, Entry> _entries = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _tryAcquireCounts = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _renewCounts = new(StringComparer.Ordinal);

        public Task<bool> TryAcquireAsync(
            string resource,
            string ownerToken,
            TimeSpan leaseTtl,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_sync)
            {
                IncrementCount(_tryAcquireCounts, resource);
                CleanupExpiredEntry(resource);

                if (_entries.ContainsKey(resource))
                {
                    return Task.FromResult(false);
                }

                _entries[resource] = new Entry(ownerToken, DateTime.UtcNow.Add(leaseTtl));
                return Task.FromResult(true);
            }
        }

        public Task<bool> ReleaseAsync(
            string resource,
            string ownerToken,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_sync)
            {
                if (!TryGetActiveEntry(resource, out var entry) || entry.OwnerToken != ownerToken)
                {
                    return Task.FromResult(false);
                }

                _entries.Remove(resource);
                return Task.FromResult(true);
            }
        }

        public Task<bool> RenewAsync(
            string resource,
            string ownerToken,
            TimeSpan leaseTtl,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_sync)
            {
                if (!TryGetActiveEntry(resource, out var entry) || entry.OwnerToken != ownerToken)
                {
                    return Task.FromResult(false);
                }

                IncrementCount(_renewCounts, resource);
                _entries[resource] = entry with { ExpiresAtUtc = DateTime.UtcNow.Add(leaseTtl) };
                return Task.FromResult(true);
            }
        }

        public bool Exists(string resource)
        {
            lock (_sync)
            {
                return TryGetActiveEntry(resource, out _);
            }
        }

        public int GetTryAcquireCount(string resource)
        {
            lock (_sync)
            {
                return _tryAcquireCounts.GetValueOrDefault(resource);
            }
        }

        public int GetRenewCount(string resource)
        {
            lock (_sync)
            {
                return _renewCounts.GetValueOrDefault(resource);
            }
        }

        public void ResetTryAcquireCount(string resource)
        {
            lock (_sync)
            {
                _tryAcquireCounts[resource] = 0;
            }
        }

        private bool TryGetActiveEntry(string resource, out Entry entry)
        {
            CleanupExpiredEntry(resource);

            if (_entries.TryGetValue(resource, out entry!))
            {
                return true;
            }

            entry = default!;
            return false;
        }

        private void CleanupExpiredEntry(string resource)
        {
            if (_entries.TryGetValue(resource, out var entry) && entry.ExpiresAtUtc <= DateTime.UtcNow)
            {
                _entries.Remove(resource);
            }
        }

        private static void IncrementCount(IDictionary<string, int> counts, string resource)
        {
            counts[resource] = counts.TryGetValue(resource, out var currentCount)
                ? currentCount + 1
                : 1;
        }

        private sealed record Entry(string OwnerToken, DateTime ExpiresAtUtc);
    }
}

