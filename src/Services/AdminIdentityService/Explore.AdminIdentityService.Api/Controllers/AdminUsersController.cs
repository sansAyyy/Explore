using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Abstractions;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.AdminIdentityService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin-users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserAppService _adminUserAppService;

    public AdminUsersController(IAdminUserAppService adminUserAppService)
    {
        _adminUserAppService = adminUserAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync([FromQuery] GetPagedAdminUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.GetPagedAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result, value => CreatedAtAction(nameof(GetByIdAsync), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ChangePasswordAsync(Guid id, [FromBody] ChangeAdminUserPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.ChangePasswordAsync(id, request, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("{id:guid}/enable")]
    public async Task<IActionResult> ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.ActivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.DeactivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


