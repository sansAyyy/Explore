using BuildingBlocks.Common.Pagination;

namespace Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;

public sealed class GetPagedMessageTemplatesRequest : PagedRequest
{
    public string? Keyword { get; set; }

    public bool? IsEnabled { get; set; }
}

