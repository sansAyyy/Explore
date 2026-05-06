using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Abstractions.Notifications;

public interface INotificationChannelSender
{
    NotificationChannelType ChannelType { get; }

    Task<ChannelSendResult> SendAsync(ChannelSendContext context, CancellationToken cancellationToken);
}

