using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.CustomerAccountService.Application.Features.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.CustomerAccountService.Api.Controllers;

[ApiController]
[Route("api/customer-auth")]
public sealed class CustomerAuthenticationController : ControllerBase
{
    private readonly ICustomerAuthenticationAppService _authenticationAppService;

    public CustomerAuthenticationController(ICustomerAuthenticationAppService authenticationAppService)
    {
        _authenticationAppService = authenticationAppService;
    }

    [AllowAnonymous]
    [HttpPost("phone/code")]
    public async Task<IActionResult> SendPhoneLoginCodeAsync(
        [FromBody] SendCustomerPhoneLoginCodeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.SendPhoneLoginCodeAsync(request, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [AllowAnonymous]
    [HttpPost("phone/login")]
    public async Task<IActionResult> PhoneLoginAsync(
        [FromBody] CustomerPhoneLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.PhoneLoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(
        [FromBody] CustomerRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.RefreshAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(
        [FromBody] CustomerLogoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationAppService.LogoutAsync(request, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


