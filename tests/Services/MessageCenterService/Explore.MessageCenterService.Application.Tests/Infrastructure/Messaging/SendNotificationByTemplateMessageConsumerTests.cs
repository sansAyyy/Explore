using BuildingBlocks.Messaging.Abstractions.Envelope;
using BuildingBlocks.Messaging.Inbox.Abstractions;
using Explore.Contracts.Messaging.Notifications;
using Explore.MessageCenterService.Application.Features.Notifications.Abstractions;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Responses;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Infrastructure.Messaging.Consumers;

namespace Explore.MessageCenterService.Application.Tests.Infrastructure.Messaging;

public sealed class SendNotificationByTemplateMessageConsumerTests
{
    [Fact]
    public async Task HandleAsync_ShouldMapMessageToNotificationRequest()
    {
        var appService = new FakeNotificationAppService(Result.Success(new SendNotificationResponse(
            Guid.NewGuid(),
            new NotificationDispatchResponse(Guid.NewGuid(), NotificationChannelType.SiteMessage, Domain.NotificationDispatches.NotificationDispatchStatus.Delivered, null))));
        var inboxProcessor = new FakeInboxMessageProcessor();
        var consumer = new SendNotificationByTemplateMessageConsumer(appService, inboxProcessor);

        var envelope = MessageEnvelope<SendNotificationByTemplateMessage>.Create(
            new SendNotificationByTemplateMessage(
                "admin_identity.admin_user_created.site_message",
                3,
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["userName"] = "ops.admin",
                    ["operatorAdminUserId"] = "system"
                },
                "admin_identity:admin_user_created:11111111-1111-1111-1111-111111111111:1"));

        await consumer.HandleAsync(envelope, CancellationToken.None);

        var request = Assert.Single(appService.Requests);
        Assert.Equal("admin_identity.admin_user_created.site_message", request.TemplateCode);
        Assert.Equal(NotificationChannelType.SiteMessage, request.Channel);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), request.RecipientUserId);
        Assert.Equal("ops.admin", request.Parameters["userName"]);
        Assert.Equal(nameof(SendNotificationByTemplateMessageConsumer), inboxProcessor.ConsumerName);
        Assert.Equal(envelope.MessageId, inboxProcessor.MessageId);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapExplicitRecipient_WhenMessageProvidesAddress()
    {
        var appService = new FakeNotificationAppService(Result.Success(new SendNotificationResponse(
            Guid.NewGuid(),
            new NotificationDispatchResponse(Guid.NewGuid(), NotificationChannelType.Sms, Domain.NotificationDispatches.NotificationDispatchStatus.Placeholder, "placeholder"))));
        var consumer = new SendNotificationByTemplateMessageConsumer(appService, new FakeInboxMessageProcessor());

        await consumer.HandleAsync(
            MessageEnvelope<SendNotificationByTemplateMessage>.Create(
                new SendNotificationByTemplateMessage(
                    "customer_auth.phone_login_code.sms",
                    1,
                    Guid.Empty,
                    new Dictionary<string, string> { ["code"] = "666666" },
                    "customer_auth:phone_login_code:13800138000",
                    new NotificationRecipientMessage(null, "13800138000", null))),
            CancellationToken.None);

        var request = Assert.Single(appService.Requests);
        Assert.Equal(NotificationChannelType.Sms, request.Channel);
        Assert.NotNull(request.Recipient);
        Assert.Equal("13800138000", request.Recipient!.PhoneNumber);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenApplicationServiceFails()
    {
        var appService = new FakeNotificationAppService(Result.Failure<SendNotificationResponse>(Error.Validation("Template is disabled.")));
        var consumer = new SendNotificationByTemplateMessageConsumer(appService, new FakeInboxMessageProcessor());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => consumer.HandleAsync(
            MessageEnvelope<SendNotificationByTemplateMessage>.Create(
                new SendNotificationByTemplateMessage(
                    "admin_identity.admin_user_created.site_message",
                    3,
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    new Dictionary<string, string>(),
                    null)),
            CancellationToken.None));

        Assert.Contains("Template is disabled.", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCallApplicationService_WhenInboxMarksDuplicate()
    {
        var appService = new FakeNotificationAppService(Result.Success(new SendNotificationResponse(
            Guid.NewGuid(),
            new NotificationDispatchResponse(Guid.NewGuid(), NotificationChannelType.SiteMessage, Domain.NotificationDispatches.NotificationDispatchStatus.Delivered, null))));
        var consumer = new SendNotificationByTemplateMessageConsumer(appService, new FakeInboxMessageProcessor(false));

        await consumer.HandleAsync(
            MessageEnvelope<SendNotificationByTemplateMessage>.Create(
                new SendNotificationByTemplateMessage(
                    "admin_identity.admin_user_created.site_message",
                    3,
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    new Dictionary<string, string>(),
                    null)),
            CancellationToken.None);

        Assert.Empty(appService.Requests);
    }

    private sealed class FakeNotificationAppService : INotificationAppService
    {
        private readonly Result<SendNotificationResponse> _result;

        public FakeNotificationAppService(Result<SendNotificationResponse> result)
        {
            _result = result;
        }

        public List<SendNotificationRequest> Requests { get; } = [];

        public Task<Result<SendNotificationResponse>> SendByTemplateAsync(SendNotificationRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeInboxMessageProcessor : IInboxMessageProcessor
    {
        private readonly bool _shouldProcess;

        public FakeInboxMessageProcessor(bool shouldProcess = true)
        {
            _shouldProcess = shouldProcess;
        }

        public string? ConsumerName { get; private set; }

        public Guid MessageId { get; private set; }

        public async Task<bool> ProcessAsync<TMessage>(
            MessageEnvelope<TMessage> envelope,
            string consumerName,
            Func<CancellationToken, Task> handler,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            ConsumerName = consumerName;
            MessageId = envelope.MessageId;

            if (!_shouldProcess)
            {
                return false;
            }

            await handler(cancellationToken);
            return true;
        }
    }
}



