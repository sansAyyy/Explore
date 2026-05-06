namespace Explore.MessageCenterService.Application.Features.Notifications.Dtos.Responses;

public sealed record SendNotificationResponse(
    Guid RequestId,
    NotificationDispatchResponse Dispatch);

