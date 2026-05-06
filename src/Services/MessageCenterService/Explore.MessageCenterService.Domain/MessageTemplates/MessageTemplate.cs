using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.MessageCenterService.Domain.MessageTemplates;

public class MessageTemplate : AuditableEntity<Guid>
{
    private MessageTemplate()
    {
    }

    private MessageTemplate(
        Guid id,
        string code,
        string name,
        string? description,
        bool isEnabled,
        NotificationChannelType channelType,
        string? titleTemplate,
        string bodyTemplate)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = Require(name, nameof(name), 128);
        Description = NormalizeOptional(description, nameof(description), 256);
        IsEnabled = isEnabled;
        ChannelType = channelType;
        TitleTemplate = NormalizeOptional(titleTemplate, nameof(titleTemplate), 256);
        BodyTemplate = Require(bodyTemplate, nameof(bodyTemplate), 4000);
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsEnabled { get; private set; }

    public NotificationChannelType ChannelType { get; private set; }

    public string? TitleTemplate { get; private set; }

    public string BodyTemplate { get; private set; } = string.Empty;

    public static MessageTemplate Create(
        Guid id,
        string code,
        string name,
        string? description,
        bool isEnabled,
        NotificationChannelType channelType,
        string? titleTemplate,
        string bodyTemplate)
    {
        return new MessageTemplate(id, code, name, description, isEnabled, channelType, titleTemplate, bodyTemplate);
    }

    public void Update(
        string code,
        string name,
        string? description,
        bool isEnabled,
        NotificationChannelType channelType,
        string? titleTemplate,
        string bodyTemplate)
    {
        Code = NormalizeCode(code);
        Name = Require(name, nameof(name), 128);
        Description = NormalizeOptional(description, nameof(description), 256);
        IsEnabled = isEnabled;
        ChannelType = channelType;
        TitleTemplate = NormalizeOptional(titleTemplate, nameof(titleTemplate), 256);
        BodyTemplate = Require(bodyTemplate, nameof(bodyTemplate), 4000);
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    private static string NormalizeCode(string value)
    {
        var normalizedValue = Require(value, nameof(Code), 128).ToLowerInvariant();
        if (normalizedValue.Any(ch => !(char.IsLower(ch) || char.IsDigit(ch) || ch is '.' or '_' or '-')))
        {
            throw new DomainException("Code format is invalid.");
        }

        return normalizedValue;
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

