using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Abstractions;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.CustomerAccountService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin-customers")]
public sealed class AdminCustomersController : ControllerBase
{
    private readonly IAdminCustomerAppService _adminCustomerAppService;

    public AdminCustomersController(IAdminCustomerAppService adminCustomerAppService)
    {
        _adminCustomerAppService = adminCustomerAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync([FromQuery] GetPagedAdminCustomersRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminCustomerAppService.GetPagedAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminCustomerAppService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/enable")]
    public async Task<IActionResult> ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminCustomerAppService.ActivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminCustomerAppService.DeactivateAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


