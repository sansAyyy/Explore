using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.MessageCenterService.Application.Features.SiteMessages.Abstractions;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.MessageCenterService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/site-messages")]
public sealed class SiteMessagesController : ControllerBase
{
    private readonly ISiteMessageAppService _siteMessageAppService;

    public SiteMessagesController(ISiteMessageAppService siteMessageAppService)
    {
        _siteMessageAppService = siteMessageAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync([FromQuery] GetPagedSiteMessagesRequest request, CancellationToken cancellationToken)
    {
        var result = await _siteMessageAppService.GetPagedAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _siteMessageAppService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _siteMessageAppService.MarkReadAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllReadAsync(CancellationToken cancellationToken)
    {
        var result = await _siteMessageAppService.MarkAllReadAsync(cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


