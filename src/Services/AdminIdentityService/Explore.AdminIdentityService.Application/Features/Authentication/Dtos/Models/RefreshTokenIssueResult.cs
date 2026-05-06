namespace Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Models;

public sealed record RefreshTokenIssueResult(
    string RefreshToken,
    DateTime ExpiresAt);

