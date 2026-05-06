using BuildingBlocks.Common.Results;
using System.ComponentModel.DataAnnotations;

namespace Explore.CustomerAccountService.Application.Features.CurrentCustomer;

internal static class CurrentCustomerRequestValidator
{
    public static Error? Validate(UpdateCurrentCustomerProfileRequest request)
    {
        var nickNameError = ValidateRequiredText(request.NickName, "NickName", 64);
        if (nickNameError is not null)
        {
            return nickNameError;
        }

        var avatarUrlError = ValidateOptionalText(request.AvatarUrl, "AvatarUrl", 512);
        if (avatarUrlError is not null)
        {
            return avatarUrlError;
        }

        var emailError = ValidateOptionalText(request.Email, "Email", 256);
        if (emailError is not null)
        {
            return emailError;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !new EmailAddressAttribute().IsValid(request.Email.Trim()))
        {
            return Error.Validation("Email format is invalid.");
        }

        return null;
    }

    public static Error? Validate(UpdateCurrentCustomerAvatarRequest request)
    {
        return ValidateAvatarUrl(request.AvatarUrl);
    }

    public static Error? ValidateAvatarUrl(string? avatarUrl)
    {
        return ValidateOptionalText(avatarUrl, "AvatarUrl", 512);
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

    private static Error? ValidateOptionalText(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().Length > maxLength
            ? Error.Validation($"{fieldName} exceeds max length {maxLength}.")
            : null;
    }
}

