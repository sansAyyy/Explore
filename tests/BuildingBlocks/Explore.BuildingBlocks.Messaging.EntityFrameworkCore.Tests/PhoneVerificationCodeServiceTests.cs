using System.Text.Json;
using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Security.PhoneVerification.Models;
using BuildingBlocks.Security.PhoneVerification.Options;
using BuildingBlocks.Security.PhoneVerification.Services;
using Microsoft.Extensions.Options;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class PhoneVerificationCodeServiceTests
{
    [Fact]
    public async Task IssueAsync_ShouldStoreHashedCodeAndReturnPlaintextCode()
    {
        var cache = new FakeCacheService();
        var service = CreateService(cache);

        var result = await service.IssueAsync("customer-auth", "13800138000", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Matches("^[0-9]{6}$", result.Value!.Code);
        Assert.True(await cache.ExistsAsync("customer-auth:phone-verification:code:13800138000", CancellationToken.None));

        var rawCacheValue = cache.GetRawValue("customer-auth:phone-verification:code:13800138000");
        Assert.NotNull(rawCacheValue);
        Assert.DoesNotContain(result.Value.Code, JsonSerializer.Serialize(rawCacheValue, rawCacheValue.GetType()));
    }

    [Fact]
    public async Task IssueAsync_ShouldReturnFailure_WhenRequestedTooFrequently()
    {
        var cache = new FakeCacheService();
        var service = CreateService(cache);

        var firstResult = await service.IssueAsync("customer-auth", "13800138000", CancellationToken.None);
        var secondResult = await service.IssueAsync("customer-auth", "13800138000", CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailure);
        Assert.Equal(ErrorCodes.BadRequest, secondResult.Error.Code);
    }

    [Fact]
    public async Task VerifyAsync_ShouldConsumeCode_WhenVerificationSucceeds()
    {
        var cache = new FakeCacheService();
        var service = CreateService(cache);
        var issueResult = await service.IssueAsync("customer-auth", "13800138000", CancellationToken.None);

        var verifyResult = await service.VerifyAsync(
            "customer-auth",
            "13800138000",
            issueResult.Value!.Code,
            CancellationToken.None);

        Assert.Equal(PhoneVerificationCodeVerificationStatus.Verified, verifyResult);
        Assert.False(await cache.ExistsAsync("customer-auth:phone-verification:code:13800138000", CancellationToken.None));
    }

    [Fact]
    public async Task VerifyAsync_ShouldExpireCode_WhenMaxAttemptsIsReached()
    {
        var cache = new FakeCacheService();
        var service = CreateService(cache);
        var issueResult = await service.IssueAsync("customer-auth", "13800138000", CancellationToken.None);
        var invalidCode = issueResult.Value!.Code == "000000" ? "111111" : "000000";

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            var verifyResult = await service.VerifyAsync("customer-auth", "13800138000", invalidCode, CancellationToken.None);
            Assert.Equal(PhoneVerificationCodeVerificationStatus.Invalid, verifyResult);
        }

        var exhaustedResult = await service.VerifyAsync("customer-auth", "13800138000", invalidCode, CancellationToken.None);
        var replayResult = await service.VerifyAsync(
            "customer-auth",
            "13800138000",
            issueResult.Value.Code,
            CancellationToken.None);

        Assert.Equal(PhoneVerificationCodeVerificationStatus.AttemptsExceeded, exhaustedResult);
        Assert.Equal(PhoneVerificationCodeVerificationStatus.MissingOrExpired, replayResult);
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnMissingOrExpired_WhenCodeHasExpired()
    {
        var cache = new FakeCacheService();
        var service = CreateService(
            cache,
            new PhoneVerificationCodeOptions
            {
                CodeLength = 6,
                CodeTtl = TimeSpan.FromMilliseconds(10),
                SendInterval = TimeSpan.FromMilliseconds(10),
                MaxVerifyAttempts = 5
            });
        var issueResult = await service.IssueAsync("customer-auth", "13800138000", CancellationToken.None);

        await Task.Delay(50);

        var verifyResult = await service.VerifyAsync(
            "customer-auth",
            "13800138000",
            issueResult.Value!.Code,
            CancellationToken.None);

        Assert.Equal(PhoneVerificationCodeVerificationStatus.MissingOrExpired, verifyResult);
    }

    [Fact]
    public async Task InvalidateAsync_ShouldRemoveIssuedCodeAndRateLimit()
    {
        var cache = new FakeCacheService();
        var service = CreateService(cache);
        var issueResult = await service.IssueAsync("customer-auth", "13800138000", CancellationToken.None);

        await service.InvalidateAsync("customer-auth", "13800138000", CancellationToken.None);

        var verifyResult = await service.VerifyAsync(
            "customer-auth",
            "13800138000",
            issueResult.Value!.Code,
            CancellationToken.None);

        Assert.False(await cache.ExistsAsync("customer-auth:phone-verification:code:13800138000", CancellationToken.None));
        Assert.False(await cache.ExistsAsync("customer-auth:phone-verification:send-interval:13800138000", CancellationToken.None));
        Assert.Equal(PhoneVerificationCodeVerificationStatus.MissingOrExpired, verifyResult);
    }

    private static PhoneVerificationCodeService CreateService(
        FakeCacheService cache,
        PhoneVerificationCodeOptions? options = null)
    {
        return new PhoneVerificationCodeService(
            cache,
            Options.Create(options ?? new PhoneVerificationCodeOptions()));
    }

    private sealed class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);

        public object? GetRawValue(string key)
        {
            return TryGetEntry(key, out var entry) ? entry.Value : null;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            if (!TryGetEntry(key, out var entry))
            {
                return Task.FromResult(default(T));
            }

            return Task.FromResult((T?)entry.Value);
        }

        public Task SetAsync<T>(string key, T value, CancellationToken ct = default)
        {
            _entries[key] = new CacheEntry(value, null);
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
        {
            _entries[key] = new CacheEntry(value, DateTime.UtcNow.Add(ttl));
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _entries.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            return Task.FromResult(TryGetEntry(key, out _));
        }

        private bool TryGetEntry(string key, out CacheEntry entry)
        {
            if (_entries.TryGetValue(key, out entry!) &&
                (!entry.ExpiresAt.HasValue || entry.ExpiresAt.Value > DateTime.UtcNow))
            {
                return true;
            }

            _entries.Remove(key);
            entry = default!;
            return false;
        }

        private sealed record CacheEntry(object? Value, DateTime? ExpiresAt);
    }
}

