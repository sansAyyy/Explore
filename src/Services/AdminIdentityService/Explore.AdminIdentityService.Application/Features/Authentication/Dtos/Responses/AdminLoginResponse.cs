namespace Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Responses;

public sealed record AdminLoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt);

