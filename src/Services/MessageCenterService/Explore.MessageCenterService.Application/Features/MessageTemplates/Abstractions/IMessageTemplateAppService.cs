using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Responses;

namespace Explore.MessageCenterService.Application.Features.MessageTemplates.Abstractions;

public interface IMessageTemplateAppService
{
    Task<Result<PagedResult<MessageTemplateBasicResponse>>> GetPagedAsync(
        GetPagedMessageTemplatesRequest request,
        CancellationToken cancellationToken);

    Task<Result<MessageTemplateDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<MessageTemplateDetailResponse>> CreateAsync(
        CreateMessageTemplateRequest request,
        CancellationToken cancellationToken);

    Task<Result<MessageTemplateDetailResponse>> UpdateAsync(
        Guid id,
        UpdateMessageTemplateRequest request,
        CancellationToken cancellationToken);

    Task<Result> EnableAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DisableAsync(Guid id, CancellationToken cancellationToken);
}

