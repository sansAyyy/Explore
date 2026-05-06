using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Notifications;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Domain.NotificationDispatches;

namespace Explore.MessageCenterService.Infrastructure.Notifications;

public sealed class SmsPlaceholderSender : INotificationChannelSender, IScopeDependency
{
    public NotificationChannelType ChannelType => NotificationChannelType.Sms;

    public Task<ChannelSendResult> SendAsync(ChannelSendContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelSendResult(
            NotificationDispatchStatus.Placeholder,
            "SMS provider integration is not configured yet."));
    }
}

