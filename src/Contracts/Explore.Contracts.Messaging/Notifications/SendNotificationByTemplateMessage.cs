namespace Explore.Contracts.Messaging.Notifications;

public sealed record SendNotificationByTemplateMessage(
    string TemplateCode,
    int Channel,
    Guid RecipientUserId,
    IReadOnlyDictionary<string, string> Parameters,
    string? BusinessIdempotencyKey,
    NotificationRecipientMessage? Recipient = null);