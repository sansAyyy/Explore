using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Abstractions;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.AdminIdentityService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin-current-user")]
public sealed class AdminCurrentUserController : ControllerBase
{
    private readonly ICurrentAdminAppService _currentAdminAppService;

    public AdminCurrentUserController(ICurrentAdminAppService currentAdminAppService)
    {
        _currentAdminAppService = currentAdminAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var result = await _currentAdminAppService.GetCurrentAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateCurrentAdminProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _currentAdminAppService.UpdateProfileAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangeCurrentAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _currentAdminAppService.ChangePasswordAsync(request, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


