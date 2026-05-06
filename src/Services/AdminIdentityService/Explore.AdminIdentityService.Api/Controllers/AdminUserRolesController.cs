using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.AdminIdentityService.Application.Features.UserRoles.Abstractions;
using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.AdminIdentityService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin-users/{userId:guid}/roles")]
public sealed class AdminUserRolesController : ControllerBase
{
    private readonly IAdminUserRoleAppService _adminUserRoleAppService;

    public AdminUserRolesController(IAdminUserRoleAppService adminUserRoleAppService)
    {
        _adminUserRoleAppService = adminUserRoleAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _adminUserRoleAppService.GetByUserIdAsync(userId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> AssignAsync(Guid userId, [FromBody] AssignUserRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserRoleAppService.AssignAsync(userId, request, cancellationToken);
        return this.ToActionResult(result);
    }
}


