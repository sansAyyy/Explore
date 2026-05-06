using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Features.MessageTemplates.Validators;

internal static class MessageTemplateRequestValidator
{
    private const int MaximumPageSize = 100;

    public static Error? Validate(GetPagedMessageTemplatesRequest request)
    {
        if (request.PageIndex <= 0)
        {
            return Error.Validation("PageIndex must be greater than 0.");
        }

        if (request.PageSize <= 0 || request.PageSize > MaximumPageSize)
        {
            return Error.Validation($"PageSize must be between 1 and {MaximumPageSize}.");
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword) && request.Keyword.Trim().Length > 128)
        {
            return Error.Validation("Keyword exceeds max length 128.");
        }

        return null;
    }

    public static Error? Validate(CreateMessageTemplateRequest request)
    {
        return ValidateCore(request.Code, request.Name, request.Description, request.ChannelType, request.TitleTemplate, request.BodyTemplate);
    }

    public static Error? Validate(UpdateMessageTemplateRequest request)
    {
        return ValidateCore(request.Code, request.Name, request.Description, request.ChannelType, request.TitleTemplate, request.BodyTemplate);
    }

    private static Error? ValidateCore(
        string code,
        string name,
        string? description,
        NotificationChannelType channelType,
        string? titleTemplate,
        string bodyTemplate)
    {
        var codeError = ValidateRequiredText(code, "Code", 128);
        if (codeError is not null)
        {
            return codeError;
        }

        var trimmedCode = code.Trim();
        if (trimmedCode.Any(ch => !(char.IsLower(ch) || char.IsDigit(ch) || ch is '.' or '_' or '-')))
        {
            return Error.Validation("Code format is invalid.");
        }

        var nameError = ValidateRequiredText(name, "Name", 128);
        if (nameError is not null)
        {
            return nameError;
        }

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 256)
        {
            return Error.Validation("Description exceeds max length 256.");
        }

        if (!Enum.IsDefined(channelType))
        {
            return Error.Validation("ChannelType is invalid.");
        }

        var bodyError = ValidateRequiredText(bodyTemplate, "BodyTemplate", 4000);
        if (bodyError is not null)
        {
            return bodyError;
        }

        if (!string.IsNullOrWhiteSpace(titleTemplate) && titleTemplate.Trim().Length > 256)
        {
            return Error.Validation("TitleTemplate exceeds max length 256.");
        }

        return null;
    }

    private static Error? ValidateRequiredText(string value, string fieldName, int maxLength)
    {
        var trimmedValue = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return Error.Validation($"{fieldName} is required.");
        }

        if (trimmedValue.Length > maxLength)
        {
            return Error.Validation($"{fieldName} exceeds max length {maxLength}.");
        }

        return null;
    }
}

