using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.MessageCenterService.Domain.SiteMessages;

public class SiteMessage : AuditableEntity<Guid>
{
    private SiteMessage()
    {
    }

    private SiteMessage(
        Guid id,
        Guid dispatchId,
        Guid userId,
        string? title,
        string content)
    {
        Id = id;
        DispatchId = dispatchId;
        UserId = userId;
        Title = NormalizeOptional(title, nameof(title), 256);
        Content = Require(content, nameof(content), 4000);
    }

    public Guid DispatchId { get; private set; }

    public Guid UserId { get; private set; }

    public string? Title { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public bool IsRead { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public static SiteMessage Create(
        Guid id,
        Guid dispatchId,
        Guid userId,
        string? title,
        string content)
    {
        return new SiteMessage(id, dispatchId, userId, title, content);
    }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = DateTime.UtcNow;
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

