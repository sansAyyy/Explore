using BuildingBlocks.Messaging.Outbox.Abstractions;
using Explore.Contracts.Messaging.Notifications;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.AdminIdentityService.Infrastructure.MessageCenter;
using Microsoft.Extensions.Logging.Abstractions;

namespace Explore.AdminIdentityService.Application.Tests.Infrastructure;

public sealed class MessageCenterSiteMessageSenderTests
{
    [Fact]
    public async Task SendAsync_ShouldWriteExpectedOutboxMessage()
    {
        var outboxWriter = new CapturingOutboxWriter();
        var sender = new MessageCenterSiteMessageSender(
            outboxWriter,
            NullLogger<MessageCenterSiteMessageSender>.Instance);

        var result = await sender.SendAsync(
            new AdminSiteMessageRequest(
                AdminIdentitySiteMessageTemplateCodes.AdminUserCreated,
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["adminUserId"] = "11111111-1111-1111-1111-111111111111",
                    ["userName"] = "ops.admin",
                    ["displayName"] = "Operations Admin",
                    ["operatorAdminUserId"] = "system",
                    ["email"] = "ops.admin@explore.local"
                },
                "admin_identity:admin_user_created:11111111-1111-1111-1111-111111111111:1"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(outboxWriter.Message);
        Assert.Equal("admin_identity.admin_user_created.site_message", outboxWriter.Message!.TemplateCode);
        Assert.Equal(3, outboxWriter.Message.Channel);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), outboxWriter.Message.RecipientUserId);
        Assert.Equal("system", outboxWriter.Message.Parameters["operatorAdminUserId"]);
        Assert.Equal(
            "admin_identity:admin_user_created:11111111-1111-1111-1111-111111111111:1",
            outboxWriter.Message.BusinessIdempotencyKey);
    }

    private sealed class CapturingOutboxWriter : IOutboxMessageWriter
    {
        public SendNotificationByTemplateMessage? Message { get; private set; }

        public Task WriteAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
        {
            Message = Assert.IsType<SendNotificationByTemplateMessage>(message);
            return Task.CompletedTask;
        }
    }
}


