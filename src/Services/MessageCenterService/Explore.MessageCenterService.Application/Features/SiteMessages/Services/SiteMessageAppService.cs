using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.SiteMessages.Abstractions;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;
using Explore.MessageCenterService.Application.Features.SiteMessages.Validators;

namespace Explore.MessageCenterService.Application.Features.SiteMessages.Services;

public sealed class SiteMessageAppService : ISiteMessageAppService, IScopeDependency
{
    private readonly ISiteMessageCommandRepository _commandRepository;
    private readonly ISiteMessageQueryRepository _queryRepository;
    private readonly IMessageCenterUnitOfWork _unitOfWork;

    public SiteMessageAppService(
        ISiteMessageCommandRepository commandRepository,
        ISiteMessageQueryRepository queryRepository,
        IMessageCenterUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<SiteMessageBasicResponse>>> GetPagedAsync(
        GetPagedSiteMessagesRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = SiteMessageRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<PagedResult<SiteMessageBasicResponse>>(validationError);
        }

        var pagedResult = await _queryRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(pagedResult);
    }

    public async Task<Result<SiteMessageDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var siteMessage = await _queryRepository.GetByIdAsync(id, cancellationToken);
        return siteMessage is null
            ? Result.Failure<SiteMessageDetailResponse>(Error.NotFound($"Site message '{id}' was not found."))
            : Result.Success(siteMessage);
    }

    public async Task<Result> MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var siteMessage = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (siteMessage is null)
        {
            return Result.Failure(Error.NotFound($"Site message '{id}' was not found."));
        }

        siteMessage.MarkAsRead();
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("UserId is required."));
        }

        var unreadMessages = await _commandRepository.GetUnreadByUserIdAsync(userId, cancellationToken);
        foreach (var unreadMessage in unreadMessages)
        {
            unreadMessage.MarkAsRead();
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

