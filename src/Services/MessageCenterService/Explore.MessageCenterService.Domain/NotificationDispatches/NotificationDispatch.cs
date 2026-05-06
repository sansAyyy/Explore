using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Domain.NotificationDispatches;

public class NotificationDispatch : AuditableEntity<Guid>
{
    private NotificationDispatch()
    {
    }

    private NotificationDispatch(
        Guid id,
        string templateCode,
        NotificationChannelType channelType,
        Guid? recipientUserId,
        string? recipientAddressSnapshot,
        string? title,
        string body,
        string? businessIdempotencyKey)
    {
        Id = id;
        TemplateCode = Require(templateCode, nameof(templateCode), 128);
        ChannelType = channelType;
        RecipientUserId = recipientUserId;
        RecipientAddressSnapshot = NormalizeOptional(recipientAddressSnapshot, nameof(recipientAddressSnapshot), 256);
        Title = NormalizeOptional(title, nameof(title), 256);
        Body = Require(body, nameof(body), 4000);
        BusinessIdempotencyKey = NormalizeOptional(businessIdempotencyKey, nameof(businessIdempotencyKey), 128);
        Status = NotificationDispatchStatus.Pending;
        RequestedAt = DateTime.UtcNow;
    }

    public string TemplateCode { get; private set; } = string.Empty;

    public NotificationChannelType ChannelType { get; private set; }

    public Guid? RecipientUserId { get; private set; }

    public string? RecipientAddressSnapshot { get; private set; }

    public string? Title { get; private set; }

    public string Body { get; private set; } = string.Empty;

    public NotificationDispatchStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime RequestedAt { get; private set; }

    public DateTime? SentAt { get; private set; }

    public string? BusinessIdempotencyKey { get; private set; }

    public static NotificationDispatch Create(
        Guid id,
        string templateCode,
        NotificationChannelType channelType,
        Guid? recipientUserId,
        string? recipientAddressSnapshot,
        string? title,
        string body,
        string? businessIdempotencyKey)
    {
        return new NotificationDispatch(
            id,
            templateCode,
            channelType,
            recipientUserId,
            recipientAddressSnapshot,
            title,
            body,
            businessIdempotencyKey);
    }

    public void MarkDelivered()
    {
        Status = NotificationDispatchStatus.Delivered;
        FailureReason = null;
        SentAt = DateTime.UtcNow;
    }

    public void MarkPlaceholder(string? message = null)
    {
        Status = NotificationDispatchStatus.Placeholder;
        FailureReason = NormalizeOptional(message, nameof(message), 256);
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = NotificationDispatchStatus.Failed;
        FailureReason = Require(reason, nameof(reason), 256);
        SentAt = null;
    }

    private static string Require(string value, string fieldName, int maxLength)
    {
        var trimmedValue = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        if (trimmedValue.Length > maxLength)
        {
            throw new DomainException($"{fieldName} exceeds max length {maxLength}.");
        }

        return trimmedValue;
    }

    private static string? NormalizeOptional(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        if (trimmedValue.Length > maxLength)
        {
            throw new DomainException($"{fieldName} exceeds max length {maxLength}.");
        }

        return trimmedValue;
    }
}

