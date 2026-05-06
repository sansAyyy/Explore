using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;

public sealed class SendNotificationRequest
{
    public string TemplateCode { get; set; } = string.Empty;

    public NotificationChannelType Channel { get; set; }

    public Guid RecipientUserId { get; set; }

    public NotificationRecipientRequest? Recipient { get; set; }

    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string? BusinessIdempotencyKey { get; set; }
}

