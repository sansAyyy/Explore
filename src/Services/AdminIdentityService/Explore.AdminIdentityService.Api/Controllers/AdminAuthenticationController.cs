using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.AdminIdentityService.Application.Features.Authentication.Abstractions;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.AdminIdentityService.Api.Controllers;

[ApiController]
[Route("api/admin-auth")]
public sealed class AdminAuthenticationController : ControllerBase
{
    private readonly IAdminAuthenticationAppService _authenticationAppService;

    public AdminAuthenticationController(IAdminAuthenticationAppService authenticationAppService)
    {
        _authenticationAppService = authenticationAppService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] AdminLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.LoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("phone/code")]
    public async Task<IActionResult> SendPhoneLoginCodeAsync([FromBody] AdminSendPhoneLoginCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.SendPhoneLoginCodeAsync(request, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [AllowAnonymous]
    [HttpPost("phone/login")]
    public async Task<IActionResult> PhoneLoginAsync([FromBody] AdminPhoneLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.PhoneLoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] AdminRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.RefreshAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync([FromBody] AdminLogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.LogoutAsync(request, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


