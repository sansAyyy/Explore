namespace Explore.Contracts.Messaging.Notifications;

public sealed record NotificationRecipientMessage(
    Guid? UserId,
    string? PhoneNumber,
    string? MiniProgramOpenId);
