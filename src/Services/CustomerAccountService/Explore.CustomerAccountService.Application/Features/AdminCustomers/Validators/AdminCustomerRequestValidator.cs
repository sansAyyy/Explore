using BuildingBlocks.Common.Results;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;

namespace Explore.CustomerAccountService.Application.Features.AdminCustomers.Validators;

internal static class AdminCustomerRequestValidator
{
    private const int MaximumKeywordLength = 128;
    private const int MaximumPageSize = 100;

    public static Error? Validate(GetPagedAdminCustomersRequest request)
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
}

