using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Security.PhoneVerification.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Models;
using BuildingBlocks.Security.PhoneVerification.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Security.PhoneVerification.Services;

public sealed class PhoneVerificationCodeService : IPhoneVerificationCodeService
{
    private readonly ICacheService _cacheService;
    private readonly PhoneVerificationCodeOptions _options;

    public PhoneVerificationCodeService(
        ICacheService cacheService,
        IOptions<PhoneVerificationCodeOptions> options)
    {
        _cacheService = cacheService;
        _options = options.Value;
        ValidateOptions(_options);
    }

    public async Task<Result<PhoneVerificationCodeIssueResult>> IssueAsync(
        string scope,
        string subject,
        CancellationToken cancellationToken)
    {
        var normalizedScope = NormalizeScope(scope);
        var normalizedSubject = NormalizeSubject(subject);
        var rateLimitKey = BuildRateLimitCacheKey(normalizedScope, normalizedSubject);
        if (await _cacheService.ExistsAsync(rateLimitKey, cancellationToken))
        {
            return Result.Failure<PhoneVerificationCodeIssueResult>(
                Error.BadRequest("Verification code requested too frequently. Please try again in 60 seconds."));
        }

        var code = "666666"; //GenerateCode();
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(_options.CodeTtl);
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var entry = new PhoneVerificationCodeCacheEntry(
            salt,
            ComputeCodeHash(salt, code),
            0,
            expiresAt);

        await _cacheService.SetAsync(
            BuildCodeCacheKey(normalizedScope, normalizedSubject),
            entry,
            _options.CodeTtl,
            cancellationToken);
        await _cacheService.SetAsync(
            rateLimitKey,
            now,
            _options.SendInterval,
            cancellationToken);

        return Result.Success(new PhoneVerificationCodeIssueResult(code, expiresAt));
    }

    public async Task<PhoneVerificationCodeVerificationStatus> VerifyAsync(
        string scope,
        string subject,
        string submittedCode,
        CancellationToken cancellationToken)
    {
        var normalizedScope = NormalizeScope(scope);
        var normalizedSubject = NormalizeSubject(subject);
        var codeCacheKey = BuildCodeCacheKey(normalizedScope, normalizedSubject);
        var entry = await _cacheService.GetAsync<PhoneVerificationCodeCacheEntry>(codeCacheKey, cancellationToken);
        if (entry is null)
        {
            return PhoneVerificationCodeVerificationStatus.MissingOrExpired;
        }

        var now = DateTime.UtcNow;
        if (entry.ExpiresAt <= now)
        {
            await _cacheService.RemoveAsync(codeCacheKey, cancellationToken);
            return PhoneVerificationCodeVerificationStatus.MissingOrExpired;
        }

        if (MatchesCodeHash(submittedCode.Trim(), entry.Salt, entry.CodeHash))
        {
            await _cacheService.RemoveAsync(codeCacheKey, cancellationToken);
            return PhoneVerificationCodeVerificationStatus.Verified;
        }

        var failedAttempts = entry.FailedAttempts + 1;
        if (failedAttempts >= _options.MaxVerifyAttempts)
        {
            await _cacheService.RemoveAsync(codeCacheKey, cancellationToken);
            return PhoneVerificationCodeVerificationStatus.AttemptsExceeded;
        }

        var ttl = entry.ExpiresAt - now;
        if (ttl <= TimeSpan.Zero)
        {
            await _cacheService.RemoveAsync(codeCacheKey, cancellationToken);
            return PhoneVerificationCodeVerificationStatus.MissingOrExpired;
        }

        await _cacheService.SetAsync(
            codeCacheKey,
            entry with { FailedAttempts = failedAttempts },
            ttl,
            cancellationToken);
        return PhoneVerificationCodeVerificationStatus.Invalid;
    }

    public async Task InvalidateAsync(
        string scope,
        string subject,
        CancellationToken cancellationToken)
    {
        var normalizedScope = NormalizeScope(scope);
        var normalizedSubject = NormalizeSubject(subject);

        await _cacheService.RemoveAsync(BuildCodeCacheKey(normalizedScope, normalizedSubject), cancellationToken);
        await _cacheService.RemoveAsync(BuildRateLimitCacheKey(normalizedScope, normalizedSubject), cancellationToken);
    }

    private static void ValidateOptions(PhoneVerificationCodeOptions options)
    {
        if (options.CodeLength <= 0)
        {
            throw new InvalidOperationException("PhoneVerificationCode.CodeLength must be greater than zero.");
        }

        if (options.CodeTtl <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("PhoneVerificationCode.CodeTtl must be greater than zero.");
        }

        if (options.SendInterval <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("PhoneVerificationCode.SendInterval must be greater than zero.");
        }

        if (options.MaxVerifyAttempts <= 0)
        {
            throw new InvalidOperationException("PhoneVerificationCode.MaxVerifyAttempts must be greater than zero.");
        }
    }

    private string GenerateCode()
    {
        var buffer = new char[_options.CodeLength];
        for (var index = 0; index < buffer.Length; index++)
        {
            buffer[index] = (char)('0' + RandomNumberGenerator.GetInt32(10));
        }

        return new string(buffer);
    }

    private static string ComputeCodeHash(string salt, string code)
    {
        return Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{code}")));
    }

    private static bool MatchesCodeHash(string code, string salt, string storedHash)
    {
        byte[] storedHashBytes;
        try
        {
            storedHashBytes = Convert.FromBase64String(storedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        var computedHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{code}"));
        return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHashBytes);
    }

    private static string NormalizeScope(string scope)
    {
        return scope.Trim().ToLowerInvariant();
    }

    private static string NormalizeSubject(string subject)
    {
        return subject.Trim();
    }

    private static string BuildCodeCacheKey(string scope, string subject)
    {
        return $"{scope}:phone-verification:code:{subject}";
    }

    private static string BuildRateLimitCacheKey(string scope, string subject)
    {
        return $"{scope}:phone-verification:send-interval:{subject}";
    }

    private sealed record PhoneVerificationCodeCacheEntry(
        string Salt,
        string CodeHash,
        int FailedAttempts,
        DateTime ExpiresAt);
}

