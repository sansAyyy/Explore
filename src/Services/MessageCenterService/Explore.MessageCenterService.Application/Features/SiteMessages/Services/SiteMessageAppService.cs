using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
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
    private readonly ICurrentUser _currentUser;
    private readonly ISiteMessageQueryRepository _queryRepository;
    private readonly IMessageCenterUnitOfWork _unitOfWork;

    public SiteMessageAppService(
        ISiteMessageCommandRepository commandRepository,
        ICurrentUser currentUser,
        ISiteMessageQueryRepository queryRepository,
        IMessageCenterUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _currentUser = currentUser;
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

        var currentUserIdResult = GetCurrentUserId();
        if (currentUserIdResult.IsFailure)
        {
            return Result.Failure<PagedResult<SiteMessageBasicResponse>>(currentUserIdResult.Error);
        }

        var pagedResult = await _queryRepository.GetPagedAsync(currentUserIdResult.Value, request, cancellationToken);
        return Result.Success(pagedResult);
    }

    public async Task<Result<SiteMessageDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserIdResult = GetCurrentUserId();
        if (currentUserIdResult.IsFailure)
        {
            return Result.Failure<SiteMessageDetailResponse>(currentUserIdResult.Error);
        }

        var siteMessage = await _queryRepository.GetByIdAsync(id, currentUserIdResult.Value, cancellationToken);
        return siteMessage is null
            ? Result.Failure<SiteMessageDetailResponse>(Error.NotFound($"Site message '{id}' was not found."))
            : Result.Success(siteMessage);
    }

    public async Task<Result> MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserIdResult = GetCurrentUserId();
        if (currentUserIdResult.IsFailure)
        {
            return Result.Failure(currentUserIdResult.Error);
        }

        var siteMessage = await _commandRepository.GetByIdAsync(id, currentUserIdResult.Value, cancellationToken);
        if (siteMessage is null)
        {
            return Result.Failure(Error.NotFound($"Site message '{id}' was not found."));
        }

        siteMessage.MarkAsRead();
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MarkAllReadAsync(CancellationToken cancellationToken)
    {
        var currentUserIdResult = GetCurrentUserId();
        if (currentUserIdResult.IsFailure)
        {
            return Result.Failure(currentUserIdResult.Error);
        }

        var unreadMessages = await _commandRepository.GetUnreadByUserIdAsync(currentUserIdResult.Value, cancellationToken);
        foreach (var unreadMessage in unreadMessages)
        {
            unreadMessage.MarkAsRead();
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private Result<Guid> GetCurrentUserId()
    {
        return _currentUser.UserId.HasValue
            ? Result.Success(_currentUser.UserId.Value)
            : Result.Failure<Guid>(Error.Unauthorized("Current user is not authenticated."));
    }
}

