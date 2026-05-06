namespace Explore.CustomerAccountService.Application.Features.Authentication;

public interface ICustomerAuthenticationAppService
{
    Task<BuildingBlocks.Common.Results.Result> SendPhoneLoginCodeAsync(
        SendCustomerPhoneLoginCodeRequest request,
        CancellationToken cancellationToken);

    Task<BuildingBlocks.Common.Results.Result<CustomerLoginResponse>> PhoneLoginAsync(
        CustomerPhoneLoginRequest request,
        CancellationToken cancellationToken);

    Task<BuildingBlocks.Common.Results.Result<CustomerLoginResponse>> RefreshAsync(
        CustomerRefreshTokenRequest request,
        CancellationToken cancellationToken);

    Task<BuildingBlocks.Common.Results.Result> LogoutAsync(
        CustomerLogoutRequest request,
        CancellationToken cancellationToken);
}

public interface ICustomerRefreshTokenService
{
    Task<RefreshTokenIssueResult> CreateAsync(Guid userId, CancellationToken cancellationToken);

    Task<BuildingBlocks.Common.Results.Result<ValidatedRefreshTokenSession>> ValidateAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<RefreshTokenIssueResult> RotateAsync(
        ValidatedRefreshTokenSession session,
        CancellationToken cancellationToken);

    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken);
}

public sealed class SendCustomerPhoneLoginCodeRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public sealed class CustomerPhoneLoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;

    public string VerificationCode { get; set; } = string.Empty;
}

public sealed class CustomerRefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class CustomerLogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed record CustomerLoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt);

public sealed record RefreshTokenIssueResult(
    string RefreshToken,
    DateTime ExpiresAt);

public sealed record ValidatedRefreshTokenSession(
    Guid SessionId,
    Guid UserId,
    DateTime CreatedAt,
    DateTime ExpiresAt);

