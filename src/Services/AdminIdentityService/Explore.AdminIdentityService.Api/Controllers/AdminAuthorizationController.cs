using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.AdminIdentityService.Application.Features.Authorization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.AdminIdentityService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin-authorization")]
public sealed class AdminAuthorizationController : ControllerBase
{
    private readonly IAdminAuthorizationAppService _adminAuthorizationAppService;

    public AdminAuthorizationController(IAdminAuthorizationAppService adminAuthorizationAppService)
    {
        _adminAuthorizationAppService = adminAuthorizationAppService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var result = await _adminAuthorizationAppService.GetCurrentAsync(cancellationToken);
        return this.ToActionResult(result);
    }
}


