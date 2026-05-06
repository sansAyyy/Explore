using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Messaging.Outbox.Abstractions;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.Contracts.Messaging.Notifications;
using Microsoft.Extensions.Logging;

namespace Explore.AdminIdentityService.Infrastructure.MessageCenter;

public sealed class MessageCenterSiteMessageSender : IAdminSiteMessageSender, IScopeDependency
{
    private const int SiteMessageChannel = 3;

    private readonly IOutboxMessageWriter _outboxMessageWriter;
    private readonly ILogger<MessageCenterSiteMessageSender> _logger;

    public MessageCenterSiteMessageSender(
        IOutboxMessageWriter outboxMessageWriter,
        ILogger<MessageCenterSiteMessageSender> logger)
    {
        _outboxMessageWriter = outboxMessageWriter;
        _logger = logger;
    }

    public async Task<Result> SendAsync(AdminSiteMessageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _outboxMessageWriter.WriteAsync(
                new SendNotificationByTemplateMessage(
                    request.TemplateCode,
                    SiteMessageChannel,
                    request.RecipientUserId,
                    request.Parameters,
                    request.BusinessIdempotencyKey),
                cancellationToken);
            return Result.Success();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to publish site message notification. TemplateCode: {TemplateCode}, RecipientUserId: {RecipientUserId}",
                request.TemplateCode,
                request.RecipientUserId);
            return Result.Failure(Error.BadRequest($"Message center outbox write failed: {exception.Message}"));
        }
    }
}


