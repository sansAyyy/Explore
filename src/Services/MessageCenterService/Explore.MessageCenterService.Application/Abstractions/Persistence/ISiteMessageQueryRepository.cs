using BuildingBlocks.Common.Pagination;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;

namespace Explore.MessageCenterService.Application.Abstractions.Persistence;

public interface ISiteMessageQueryRepository
{
    Task<PagedResult<SiteMessageBasicResponse>> GetPagedAsync(
        Guid userId,
        GetPagedSiteMessagesRequest request,
        CancellationToken cancellationToken);

    Task<SiteMessageDetailResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}

