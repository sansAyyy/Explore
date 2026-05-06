namespace BuildingBlocks.Security.Authentication.Models;

public sealed record AccessTokenResult(
    string AccessToken,
    DateTime ExpiresAt);

