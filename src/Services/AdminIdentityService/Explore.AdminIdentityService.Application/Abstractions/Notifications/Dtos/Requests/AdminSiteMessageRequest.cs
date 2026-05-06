namespace Explore.AdminIdentityService.Application.Abstractions.Notifications;

public sealed record AdminSiteMessageRequest(
    string TemplateCode,
    Guid RecipientUserId,
    IReadOnlyDictionary<string, string> Parameters,
    string? BusinessIdempotencyKey);

