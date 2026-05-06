using BuildingBlocks.Common.AspNetCore.Results;
using BuildingBlocks.Security.Authorization.Constants;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Abstractions;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.MessageCenterService.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/message-templates")]
public sealed class MessageTemplatesController : ControllerBase
{
    private readonly IMessageTemplateAppService _messageTemplateAppService;

    public MessageTemplatesController(IMessageTemplateAppService messageTemplateAppService)
    {
        _messageTemplateAppService = messageTemplateAppService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync([FromQuery] GetPagedMessageTemplatesRequest request, CancellationToken cancellationToken)
    {
        var result = await _messageTemplateAppService.GetPagedAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messageTemplateAppService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateMessageTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await _messageTemplateAppService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result, value => CreatedAtAction(nameof(GetByIdAsync), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateMessageTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await _messageTemplateAppService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/enable")]
    public async Task<IActionResult> EnableAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messageTemplateAppService.EnableAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }

    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> DisableAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messageTemplateAppService.DisableAsync(id, cancellationToken);
        return this.ToActionResult(result, NoContent);
    }
}


