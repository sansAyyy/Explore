using BuildingBlocks.Common.AspNetCore.Results;
using Explore.CustomerAccountService.Application.Features.NotificationProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explore.CustomerAccountService.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly INotificationProfileAppService _notificationProfileAppService;

    public UsersController(INotificationProfileAppService notificationProfileAppService)
    {
        _notificationProfileAppService = notificationProfileAppService;
    }

    [AllowAnonymous]
    [HttpGet("{userId:guid}/notification-profile")]
    public async Task<IActionResult> GetNotificationProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _notificationProfileAppService.GetByUserIdAsync(userId, cancellationToken);
        return this.ToActionResult(result);
    }
}


