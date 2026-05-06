using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Requests;
using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.CurrentAdmin.Validators;

internal static class CurrentAdminRequestValidator
{
    private const int MinimumPasswordLength = 8;

    public static Error? Validate(UpdateCurrentAdminProfileRequest request)
    {
        var userNameError = ValidateRequiredText(request.UserName, "UserName", 64);
        if (userNameError is not null)
        {
            return userNameError;
        }

        var emailError = ValidateRequiredText(request.Email, "Email", 256);
        if (emailError is not null)
        {
            return emailError;
        }

        if (!new EmailAddressAttribute().IsValid(request.Email.Trim()))
        {
            return Error.Validation("Email format is invalid.");
        }

        return ValidateRequiredText(request.DisplayName, "DisplayName", 128);
    }

    public static Error? Validate(ChangeCurrentAdminPasswordRequest request)
    {
        var currentPasswordError = ValidateRequiredText(request.CurrentPassword, "CurrentPassword", 128);
        if (currentPasswordError is not null)
        {
            return currentPasswordError;
        }

        var newPasswordError = ValidateRequiredText(request.NewPassword, "NewPassword", 128);
        if (newPasswordError is not null)
        {
            return newPasswordError;
        }

        if (request.NewPassword.Trim().Length < MinimumPasswordLength)
        {
            return Error.Validation($"NewPassword must be at least {MinimumPasswordLength} characters.");
        }

        if (string.Equals(request.CurrentPassword.Trim(), request.NewPassword.Trim(), StringComparison.Ordinal))
        {
            return Error.Validation("NewPassword must be different from CurrentPassword.");
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

