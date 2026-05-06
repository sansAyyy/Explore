using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Abstractions.Notifications;

public sealed record ChannelSendContext(
    Guid DispatchId,
    Guid? RecipientUserId,
    string RecipientAddress,
    NotificationChannelType ChannelType,
    string? Title,
    string Body);

