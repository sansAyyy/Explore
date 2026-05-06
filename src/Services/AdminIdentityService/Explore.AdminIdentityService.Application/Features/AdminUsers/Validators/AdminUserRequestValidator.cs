using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;
using System.ComponentModel.DataAnnotations;

namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Validators;

internal static class AdminUserRequestValidator
{
    private const int MaximumKeywordLength = 128;
    private const int MaximumPageSize = 100;
    private const int MinimumPasswordLength = 8;
    private const string MainlandPhoneNumberPattern = "^1\\d{10}$";

    public static Error? Validate(GetPagedAdminUsersRequest request)
    {
        if (request.PageIndex <= 0)
        {
            return Error.Validation("PageIndex must be greater than 0.");
        }

        if (request.PageSize <= 0 || request.PageSize > MaximumPageSize)
        {
            return Error.Validation($"PageSize must be between 1 and {MaximumPageSize}.");
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword) && request.Keyword.Trim().Length > MaximumKeywordLength)
        {
            return Error.Validation($"Keyword exceeds max length {MaximumKeywordLength}.");
        }

        return null;
    }

    public static Error? Validate(CreateAdminUserRequest request)
    {
        var profileError = ValidateProfile(request.UserName, request.Email, request.DisplayName, request.PhoneNumber);
        return profileError ?? ValidatePassword(request.Password, nameof(request.Password));
    }

    public static Error? Validate(UpdateAdminUserRequest request)
    {
        return ValidateProfile(request.UserName, request.Email, request.DisplayName, request.PhoneNumber);
    }

    public static Error? Validate(ChangeAdminUserPasswordRequest request)
    {
        return ValidatePassword(request.NewPassword, nameof(request.NewPassword));
    }

    private static Error? ValidateProfile(string userName, string email, string displayName, string? phoneNumber)
    {
        var userNameError = ValidateRequiredText(userName, "UserName", 64);
        if (userNameError is not null)
        {
            return userNameError;
        }

        var emailError = ValidateRequiredText(email, "Email", 256);
        if (emailError is not null)
        {
            return emailError;
        }

        if (!new EmailAddressAttribute().IsValid(email.Trim()))
        {
            return Error.Validation("Email format is invalid.");
        }

        var displayNameError = ValidateRequiredText(displayName, "DisplayName", 128);
        if (displayNameError is not null)
        {
            return displayNameError;
        }

        return ValidatePhoneNumber(phoneNumber, required: false);
    }

    private static Error? ValidatePassword(string password, string fieldName)
    {
        var passwordError = ValidateRequiredText(password, fieldName, 128);
        if (passwordError is not null)
        {
            return passwordError;
        }

        if (password.Trim().Length < MinimumPasswordLength)
        {
            return Error.Validation($"{fieldName} must be at least {MinimumPasswordLength} characters.");
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

    private static Error? ValidatePhoneNumber(string? phoneNumber, bool required)
    {
        var trimmedPhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedPhoneNumber))
        {
            return required ? Error.Validation("PhoneNumber is required.") : null;
        }

        if (trimmedPhoneNumber.Length > 32)
        {
            return Error.Validation("PhoneNumber exceeds max length 32.");
        }

        return System.Text.RegularExpressions.Regex.IsMatch(trimmedPhoneNumber, MainlandPhoneNumberPattern)
            ? null
            : Error.Validation("PhoneNumber format is invalid.");
    }
}

