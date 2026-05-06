using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.AdminIdentityService.Application.Features.Roles.Abstractions;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.AdminIdentityService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin-roles")]
public sealed class AdminRolesController : ControllerBase
{
    private readonly IAdminRoleAppService _adminRoleAppService;

    public AdminRolesController(IAdminRoleAppService adminRoleAppService)
    {
        _adminRoleAppService = adminRoleAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync([FromQuery] GetPagedAdminRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.GetPagedAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAdminRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result, value => CreatedAtAction(nameof(GetByIdAsync), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateAdminRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/enable")]
    public async Task<IActionResult> ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.ActivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.DeactivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpGet("{id:guid}/permissions")]
    public async Task<IActionResult> GetPermissionsAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.GetPermissionsAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> AssignPermissionsAsync(Guid id, [FromBody] AssignRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminRoleAppService.AssignPermissionsAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }
}


