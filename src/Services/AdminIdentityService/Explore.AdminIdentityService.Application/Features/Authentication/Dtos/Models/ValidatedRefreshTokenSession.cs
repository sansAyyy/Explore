namespace Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Models;

public sealed record ValidatedRefreshTokenSession(
    Guid SessionId,
    Guid UserId,
    DateTime CreatedAt,
    DateTime ExpiresAt);

