using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.AdminIdentityService.Application.Features.Permissions.Abstractions;
using Explore.AdminIdentityService.Application.Features.Permissions.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.AdminIdentityService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin-permissions")]
public sealed class AdminPermissionsController : ControllerBase
{
    private readonly IAdminPermissionAppService _adminPermissionAppService;

    public AdminPermissionsController(IAdminPermissionAppService adminPermissionAppService)
    {
        _adminPermissionAppService = adminPermissionAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync([FromQuery] GetPagedAdminPermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.GetPagedAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("roots")]
    public async Task<IActionResult> GetRootsAsync([FromQuery] GetAdminPermissionTreeRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.GetRootsAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}/descendants")]
    public async Task<IActionResult> GetDescendantsAsync(Guid id, [FromQuery] GetAdminPermissionTreeRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.GetDescendantsAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAdminPermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result, value => CreatedAtAction(nameof(GetByIdAsync), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateAdminPermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/enable")]
    public async Task<IActionResult> ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.ActivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.DeactivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminPermissionAppService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


