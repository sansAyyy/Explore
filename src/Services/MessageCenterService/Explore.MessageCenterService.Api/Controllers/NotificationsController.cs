using BuildingBlocks.Common.AspNetCore.Results;
using Explore.MessageCenterService.Application.Features.Notifications.Abstractions;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Explore.MessageCenterService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationAppService _notificationAppService;

    public NotificationsController(INotificationAppService notificationAppService)
    {
        _notificationAppService = notificationAppService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendAsync([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _notificationAppService.SendByTemplateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}


