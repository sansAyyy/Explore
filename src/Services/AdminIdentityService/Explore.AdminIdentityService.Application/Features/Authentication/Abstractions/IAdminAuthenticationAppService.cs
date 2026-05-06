using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Abstractions;

public interface IAdminAuthenticationAppService
{
    Task<Result<AdminLoginResponse>> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken);

    Task<Result> SendPhoneLoginCodeAsync(AdminSendPhoneLoginCodeRequest request, CancellationToken cancellationToken);

    Task<Result<AdminLoginResponse>> PhoneLoginAsync(AdminPhoneLoginRequest request, CancellationToken cancellationToken);

    Task<Result<AdminLoginResponse>> RefreshAsync(AdminRefreshTokenRequest request, CancellationToken cancellationToken);

    Task<Result> LogoutAsync(AdminLogoutRequest request, CancellationToken cancellationToken);
}

