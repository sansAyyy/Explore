namespace BuildingBlocks.Security.Authentication.Options;

public sealed class JwtOptions
{
    public const string SectionName = nameof(JwtOptions);

    public string Issuer { get; init; } = "Explore";

    public string Audience { get; init; } = "Explore.Clients";

    public string SigningKey { get; init; } = "change-this-local-jwt-signing-key";

    public int AccessTokenExpirationMinutes { get; init; } = 120;

    public int RefreshTokenExpirationDays { get; init; } = 7;
}

