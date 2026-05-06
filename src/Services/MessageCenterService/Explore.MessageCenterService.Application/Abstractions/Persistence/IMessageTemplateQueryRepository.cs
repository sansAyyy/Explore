using BuildingBlocks.Common.Pagination;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Responses;

namespace Explore.MessageCenterService.Application.Abstractions.Persistence;

public interface IMessageTemplateQueryRepository
{
    Task<PagedResult<MessageTemplateBasicResponse>> GetPagedAsync(
        GetPagedMessageTemplatesRequest request,
        CancellationToken cancellationToken);

    Task<MessageTemplateDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

