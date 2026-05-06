using BuildingBlocks.Common.Results;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Explore.CustomerAccountService.Application.Features.Authentication;

internal static class CustomerAuthenticationRequestValidator
{
    private const string MainlandPhoneNumberPattern = "^1\\d{10}$";

    public static Error? Validate(SendCustomerPhoneLoginCodeRequest request)
    {
        return ValidatePhoneNumber(request.PhoneNumber);
    }

    public static Error? Validate(CustomerPhoneLoginRequest request)
    {
        var phoneError = ValidatePhoneNumber(request.PhoneNumber);
        if (phoneError is not null)
        {
            return phoneError;
        }

        return ValidateRequiredText(request.VerificationCode, "VerificationCode", 16);
    }

    public static Error? Validate(CustomerRefreshTokenRequest request)
    {
        return ValidateRequiredText(request.RefreshToken, "RefreshToken", 1024);
    }

    public static Error? Validate(CustomerLogoutRequest request)
    {
        return ValidateRequiredText(request.RefreshToken, "RefreshToken", 1024);
    }

    private static Error? ValidatePhoneNumber(string phoneNumber)
    {
        var phoneError = ValidateRequiredText(phoneNumber, "PhoneNumber", 32);
        if (phoneError is not null)
        {
            return phoneError;
        }

        return Regex.IsMatch(phoneNumber.Trim(), MainlandPhoneNumberPattern)
            ? null
            : Error.Validation("PhoneNumber format is invalid.");
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

