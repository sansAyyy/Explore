using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Security.Authentication.Options;
using Explore.AdminIdentityService.Application.Features.Authentication.Abstractions;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Services;

public sealed class AdminRefreshTokenService : IAdminRefreshTokenService, IScopeDependency
{
    private const string RefreshTokenCacheKeyPrefix = "admin-auth:refresh:";

    private readonly ICacheService _cacheService;
    private readonly JwtOptions _jwtOptions;

    public AdminRefreshTokenService(
        ICacheService cacheService,
        IOptions<JwtOptions> jwtOptions)
    {
        _cacheService = cacheService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<RefreshTokenIssueResult> CreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid();
        var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        var cacheEntry = new RefreshTokenCacheEntry(
            sessionId,
            userId,
            ComputeSecretHash(secret),
            now,
            expiresAt);

        await _cacheService.SetAsync(
            BuildCacheKey(sessionId),
            cacheEntry,
            expiresAt - now,
            cancellationToken);

        return new RefreshTokenIssueResult(
            $"{sessionId:N}.{secret}",
            expiresAt);
    }

    public async Task<Result<ValidatedRefreshTokenSession>> ValidateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (!TryParseRefreshToken(refreshToken, out var sessionId, out var secret))
        {
            return Result.Failure<ValidatedRefreshTokenSession>(Error.Validation("RefreshToken format is invalid."));
        }

        var cacheEntry = await _cacheService.GetAsync<RefreshTokenCacheEntry>(BuildCacheKey(sessionId), cancellationToken);
        if (cacheEntry is null)
        {
            return Result.Failure<ValidatedRefreshTokenSession>(Error.Unauthorized("Refresh token is invalid."));
        }

        if (cacheEntry.ExpiresAt <= DateTime.UtcNow)
        {
            await _cacheService.RemoveAsync(BuildCacheKey(sessionId), cancellationToken);
            return Result.Failure<ValidatedRefreshTokenSession>(Error.Unauthorized("Refresh token has expired."));
        }

        if (!MatchesSecretHash(secret, cacheEntry.SecretHash))
        {
            return Result.Failure<ValidatedRefreshTokenSession>(Error.Unauthorized("Refresh token is invalid."));
        }

        return Result.Success(new ValidatedRefreshTokenSession(
            cacheEntry.SessionId,
            cacheEntry.UserId,
            cacheEntry.CreatedAt,
            cacheEntry.ExpiresAt));
    }

    public async Task<RefreshTokenIssueResult> RotateAsync(ValidatedRefreshTokenSession session, CancellationToken cancellationToken)
    {
        await RevokeAsync(session.SessionId, cancellationToken);
        return await CreateAsync(session.UserId, cancellationToken);
    }

    public Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return _cacheService.RemoveAsync(BuildCacheKey(sessionId), cancellationToken);
    }

    private static string BuildCacheKey(Guid sessionId)
    {
        return $"{RefreshTokenCacheKeyPrefix}{sessionId:N}";
    }

    private static string ComputeSecretHash(string secret)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
    }

    private static bool MatchesSecretHash(string secret, string storedHash)
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

        var computedHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHashBytes);
    }

    private static bool TryParseRefreshToken(string refreshToken, out Guid sessionId, out string secret)
    {
        sessionId = Guid.Empty;
        secret = string.Empty;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var tokenParts = refreshToken.Trim().Split('.', 2, StringSplitOptions.TrimEntries);
        if (tokenParts.Length != 2 ||
            !Guid.TryParse(tokenParts[0], out sessionId) ||
            string.IsNullOrWhiteSpace(tokenParts[1]))
        {
            sessionId = Guid.Empty;
            return false;
        }

        secret = tokenParts[1];
        return true;
    }

    private sealed record RefreshTokenCacheEntry(
        Guid SessionId,
        Guid UserId,
        string SecretHash,
        DateTime CreatedAt,
        DateTime ExpiresAt);
}

