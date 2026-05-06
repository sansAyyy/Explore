using BuildingBlocks.Common.Pagination;

namespace Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;

public sealed class GetPagedSiteMessagesRequest : PagedRequest
{
    public Guid UserId { get; set; }

    public bool? IsRead { get; set; }
}

