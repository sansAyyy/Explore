using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.Roles.Dtos.Requests;
using System.Text.RegularExpressions;

namespace Explore.AdminIdentityService.Application.Features.Roles.Validators;

internal static partial class AdminRoleRequestValidator
{
    private const int MaximumPageSize = 100;

    [GeneratedRegex("^[a-z0-9_.]+$")]
    private static partial Regex CodePattern();

    public static Error? Validate(GetPagedAdminRolesRequest request)
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

    public static Error? Validate(CreateAdminRoleRequest request) => ValidateRoleFields(request.Code, request.Name, request.Description);

    public static Error? Validate(UpdateAdminRoleRequest request) => ValidateRoleFields(request.Code, request.Name, request.Description);

    public static Error? Validate(AssignRolePermissionsRequest request)
    {
        if (request.PermissionIds.Any(x => x == Guid.Empty))
        {
            return Error.Validation("PermissionIds contains invalid id.");
        }

        return null;
    }

    private static Error? ValidateRoleFields(string code, string name, string? description)
    {
        var codeError = ValidateRequiredText(code, "Code", 128);
        if (codeError is not null)
        {
            return codeError;
        }

        if (!CodePattern().IsMatch(code.Trim()))
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

