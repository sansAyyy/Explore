using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Validators;

internal static class AdminAuthenticationRequestValidator
{
    private const string MainlandPhoneNumberPattern = "^1\\d{10}$";

    public static Error? Validate(AdminLoginRequest request)
    {
        var accountError = ValidateRequiredText(request.Account, "Account", 256);
        if (accountError is not null)
        {
            return accountError;
        }

        return ValidateRequiredText(request.Password, "Password", 128);
    }

    public static Error? Validate(AdminSendPhoneLoginCodeRequest request)
    {
        return ValidatePhoneNumber(request.PhoneNumber);
    }

    public static Error? Validate(AdminPhoneLoginRequest request)
    {
        var phoneError = ValidatePhoneNumber(request.PhoneNumber);
        if (phoneError is not null)
        {
            return phoneError;
        }

        return ValidateRequiredText(request.VerificationCode, "VerificationCode", 16);
    }

    public static Error? Validate(AdminRefreshTokenRequest request)
    {
        return ValidateRequiredText(request.RefreshToken, "RefreshToken", 1024);
    }

    public static Error? Validate(AdminLogoutRequest request)
    {
        return ValidateRequiredText(request.RefreshToken, "RefreshToken", 1024);
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

    private static Error? ValidatePhoneNumber(string phoneNumber)
    {
        var phoneError = ValidateRequiredText(phoneNumber, "PhoneNumber", 32);
        if (phoneError is not null)
        {
            return phoneError;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber.Trim(), MainlandPhoneNumberPattern)
            ? null
            : Error.Validation("PhoneNumber format is invalid.");
    }
}

