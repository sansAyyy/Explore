using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;

namespace Explore.MessageCenterService.Application.Features.SiteMessages.Abstractions;

public interface ISiteMessageAppService
{
    Task<Result<PagedResult<SiteMessageBasicResponse>>> GetPagedAsync(
        GetPagedSiteMessagesRequest request,
        CancellationToken cancellationToken);

    Task<Result<SiteMessageDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> MarkReadAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken);
}

