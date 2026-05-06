using Explore.MessageCenterService.Domain.NotificationDispatches;

namespace Explore.MessageCenterService.Application.Abstractions.Notifications;

public sealed record ChannelSendResult(
    NotificationDispatchStatus Status,
    string? Message = null);

