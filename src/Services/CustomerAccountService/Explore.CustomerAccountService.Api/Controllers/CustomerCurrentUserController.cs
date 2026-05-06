using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.CustomerAccountService.Application.Features.CurrentCustomer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.CustomerAccountService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
[Route("api/customer-current-user")]
public sealed class CustomerCurrentUserController : ControllerBase
{
    private readonly ICurrentCustomerAppService _currentCustomerAppService;

    public CustomerCurrentUserController(ICurrentCustomerAppService currentCustomerAppService)
    {
        _currentCustomerAppService = currentCustomerAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var result = await _currentCustomerAppService.GetCurrentAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateCurrentCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _currentCustomerAppService.UpdateProfileAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatarAsync(
        [FromBody] UpdateCurrentCustomerAvatarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _currentCustomerAppService.UpdateAvatarAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}


