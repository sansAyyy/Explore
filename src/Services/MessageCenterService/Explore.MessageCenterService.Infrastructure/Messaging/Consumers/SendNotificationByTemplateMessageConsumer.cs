using BuildingBlocks.Messaging.Abstractions.Envelope;
using BuildingBlocks.Messaging.Inbox.Abstractions;
using Explore.Contracts.Messaging.Notifications;
using Explore.MessageCenterService.Application.Features.Notifications.Abstractions;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;
using Explore.MessageCenterService.Domain.MessageTemplates;
using MassTransit;

namespace Explore.MessageCenterService.Infrastructure.Messaging.Consumers;

public sealed class SendNotificationByTemplateMessageConsumer
    : IConsumer<MessageEnvelope<SendNotificationByTemplateMessage>>
{
    private readonly INotificationAppService _notificationAppService;
    private readonly IInboxMessageProcessor _inboxMessageProcessor;

    public SendNotificationByTemplateMessageConsumer(
        INotificationAppService notificationAppService,
        IInboxMessageProcessor inboxMessageProcessor)
    {
        _notificationAppService = notificationAppService;
        _inboxMessageProcessor = inboxMessageProcessor;
    }

    public Task Consume(ConsumeContext<MessageEnvelope<SendNotificationByTemplateMessage>> context)
    {
        return HandleAsync(context.Message, context.CancellationToken);
    }

    public async Task HandleAsync(
        MessageEnvelope<SendNotificationByTemplateMessage> envelope,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(envelope.Payload);

        await _inboxMessageProcessor.ProcessAsync(
            envelope,
            nameof(SendNotificationByTemplateMessageConsumer),
            async ct =>
            {
                var message = envelope.Payload;
                var result = await _notificationAppService.SendByTemplateAsync(
                    new SendNotificationRequest
                    {
                        TemplateCode = message.TemplateCode,
                        Channel = (NotificationChannelType)message.Channel,
                        RecipientUserId = message.RecipientUserId,
                        Recipient = message.Recipient is null
                            ? null
                            : new NotificationRecipientRequest
                            {
                                UserId = message.Recipient.UserId,
                                PhoneNumber = message.Recipient.PhoneNumber,
                                MiniProgramOpenId = message.Recipient.MiniProgramOpenId
                            },
                        Parameters = new Dictionary<string, string>(message.Parameters, StringComparer.OrdinalIgnoreCase),
                        BusinessIdempotencyKey = message.BusinessIdempotencyKey
                    },
                    ct);

                if (result.IsFailure)
                {
                    throw new InvalidOperationException(
                        $"Failed to process send notification message. ErrorCode: {result.Error.Code}, ErrorMessage: {result.Error.Message}");
                }
            },
            cancellationToken);
    }
}



