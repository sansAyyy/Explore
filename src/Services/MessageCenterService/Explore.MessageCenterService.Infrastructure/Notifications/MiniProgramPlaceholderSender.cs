using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Notifications;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Domain.NotificationDispatches;

namespace Explore.MessageCenterService.Infrastructure.Notifications;

public sealed class MiniProgramPlaceholderSender : INotificationChannelSender, IScopeDependency
{
    public NotificationChannelType ChannelType => NotificationChannelType.MiniProgram;

    public Task<ChannelSendResult> SendAsync(ChannelSendContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ChannelSendResult(
            NotificationDispatchStatus.Placeholder,
            "Mini program provider integration is not configured yet."));
    }
}

