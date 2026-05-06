using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Domain.NotificationDispatches;

namespace Explore.MessageCenterService.Application.Features.Notifications.Dtos.Responses;

public sealed record NotificationDispatchResponse(
    Guid DispatchId,
    NotificationChannelType Channel,
    NotificationDispatchStatus Status,
    string? FailureReason);

