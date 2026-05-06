using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;

namespace Explore.MessageCenterService.Application.Features.Notifications.Validators;

internal static class NotificationRequestValidator
{
    public static Error? Validate(SendNotificationRequest request)
    {
        var templateCode = request.TemplateCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(templateCode))
        {
            return Error.Validation("TemplateCode is required.");
        }

        if (templateCode.Length > 128)
        {
            return Error.Validation("TemplateCode exceeds max length 128.");
        }

        if (!Enum.IsDefined(request.Channel))
        {
            return Error.Validation("Channel is invalid.");
        }

        if (request.Recipient is not null)
        {
            var recipientError = ValidateRecipient(request.Recipient);
            if (recipientError is not null)
            {
                return recipientError;
            }
        }

        if ((request.Recipient is null || !HasRecipientValue(request.Recipient)) &&
            request.RecipientUserId == Guid.Empty)
        {
            return Error.Validation("Recipient is required.");
        }

        foreach (var parameter in request.Parameters)
        {
            var key = parameter.Key?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                return Error.Validation("Parameter key is required.");
            }

            if (key.Length > 128)
            {
                return Error.Validation("Parameter key exceeds max length 128.");
            }

            if ((parameter.Value ?? string.Empty).Length > 4000)
            {
                return Error.Validation($"Parameter '{key}' exceeds max length 4000.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.BusinessIdempotencyKey) &&
            request.BusinessIdempotencyKey.Trim().Length > 128)
        {
            return Error.Validation("BusinessIdempotencyKey exceeds max length 128.");
        }

        return null;
    }

    private static Error? ValidateRecipient(NotificationRecipientRequest recipient)
    {
        if (!HasRecipientValue(recipient))
        {
            return Error.Validation("Recipient must include at least one address.");
        }

        if (!string.IsNullOrWhiteSpace(recipient.PhoneNumber) &&
            recipient.PhoneNumber.Trim().Length > 32)
        {
            return Error.Validation("Recipient.PhoneNumber exceeds max length 32.");
        }

        if (!string.IsNullOrWhiteSpace(recipient.MiniProgramOpenId) &&
            recipient.MiniProgramOpenId.Trim().Length > 128)
        {
            return Error.Validation("Recipient.MiniProgramOpenId exceeds max length 128.");
        }

        return null;
    }

    private static bool HasRecipientValue(NotificationRecipientRequest recipient)
    {
        return recipient.UserId.HasValue ||
            !string.IsNullOrWhiteSpace(recipient.PhoneNumber) ||
            !string.IsNullOrWhiteSpace(recipient.MiniProgramOpenId);
    }
}

