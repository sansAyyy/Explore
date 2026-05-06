using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Responses;

namespace Explore.MessageCenterService.Application.Features.Notifications.Abstractions;

public interface INotificationAppService
{
    Task<Result<SendNotificationResponse>> SendByTemplateAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken);
}

