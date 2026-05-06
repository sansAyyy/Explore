using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;

namespace Explore.MessageCenterService.Application.Features.SiteMessages.Validators;

internal static class SiteMessageRequestValidator
{
    private const int MaximumPageSize = 100;

    public static Error? Validate(GetPagedSiteMessagesRequest request)
    {
        if (request.PageIndex <= 0)
        {
            return Error.Validation("PageIndex must be greater than 0.");
        }

        if (request.PageSize <= 0 || request.PageSize > MaximumPageSize)
        {
            return Error.Validation($"PageSize must be between 1 and {MaximumPageSize}.");
        }

        return null;
    }
}

