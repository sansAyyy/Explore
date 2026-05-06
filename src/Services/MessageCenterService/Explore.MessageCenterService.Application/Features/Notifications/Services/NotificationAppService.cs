using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.MessageCenterService.Application.Abstractions.External;
using Explore.MessageCenterService.Application.Abstractions.Notifications;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.Notifications.Abstractions;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.Notifications.Dtos.Responses;
using Explore.MessageCenterService.Application.Features.Notifications.Validators;
using Explore.MessageCenterService.Domain.MessageTemplates;
using Explore.MessageCenterService.Domain.NotificationDispatches;
using Explore.MessageCenterService.Domain.SiteMessages;

namespace Explore.MessageCenterService.Application.Features.Notifications.Services;

public sealed class NotificationAppService : INotificationAppService, IScopeDependency
{
    private readonly IMessageTemplateCommandRepository _messageTemplateCommandRepository;
    private readonly INotificationDispatchCommandRepository _notificationDispatchCommandRepository;
    private readonly ISiteMessageCommandRepository _siteMessageCommandRepository;
    private readonly IRecipientDirectoryClient _recipientDirectoryClient;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IReadOnlyDictionary<NotificationChannelType, INotificationChannelSender> _channelSenders;
    private readonly IMessageCenterUnitOfWork _unitOfWork;

    public NotificationAppService(
        IMessageTemplateCommandRepository messageTemplateCommandRepository,
        INotificationDispatchCommandRepository notificationDispatchCommandRepository,
        ISiteMessageCommandRepository siteMessageCommandRepository,
        IRecipientDirectoryClient recipientDirectoryClient,
        ITemplateRenderer templateRenderer,
        IEnumerable<INotificationChannelSender> channelSenders,
        IMessageCenterUnitOfWork unitOfWork)
    {
        _messageTemplateCommandRepository = messageTemplateCommandRepository;
        _notificationDispatchCommandRepository = notificationDispatchCommandRepository;
        _siteMessageCommandRepository = siteMessageCommandRepository;
        _recipientDirectoryClient = recipientDirectoryClient;
        _templateRenderer = templateRenderer;
        _channelSenders = channelSenders.ToDictionary(x => x.ChannelType);
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SendNotificationResponse>> SendByTemplateAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = NotificationRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<SendNotificationResponse>(validationError);
        }

        var template = await _messageTemplateCommandRepository.GetByCodeAsync(
            request.TemplateCode.Trim().ToLowerInvariant(),
            cancellationToken);

        if (template is null)
        {
            return Result.Failure<SendNotificationResponse>(Error.NotFound($"Message template '{request.TemplateCode}' was not found."));
        }

        if (!template.IsEnabled)
        {
            return Result.Failure<SendNotificationResponse>(Error.Validation($"Message template '{template.Code}' is disabled."));
        }

        if (template.ChannelType != request.Channel)
        {
            return Result.Failure<SendNotificationResponse>(
                Error.Validation($"Template '{template.Code}' does not support channel '{request.Channel}'."));
        }

        var normalizedParameters = request.Parameters.ToDictionary(
            kvp => kvp.Key.Trim(),
            kvp => kvp.Value.Trim(),
            StringComparer.OrdinalIgnoreCase);
        var recipient = NormalizeRecipient(request);
        var recipientCompatibilityError = ValidateRecipientCompatibility(request.Channel, recipient);
        if (recipientCompatibilityError is not null)
        {
            return Result.Failure<SendNotificationResponse>(recipientCompatibilityError);
        }

        var recipientProfileResult = RequiresDirectoryLookup(request.Channel, recipient)
            ? await _recipientDirectoryClient.GetByUserIdAsync(recipient.UserId!.Value, cancellationToken)
            : null;

        var renderedContent = _templateRenderer.Render(
            template.TitleTemplate,
            template.BodyTemplate,
            normalizedParameters);

        if (renderedContent.IsFailure)
        {
            return Result.Failure<SendNotificationResponse>(renderedContent.Error);
        }

        var addressSnapshot = ResolveAddressSnapshot(
            request.Channel,
            recipient,
            recipientProfileResult?.Value);
        var dispatch = NotificationDispatch.Create(
            Guid.NewGuid(),
            template.Code,
            request.Channel,
            recipient.UserId,
            addressSnapshot,
            renderedContent.Value!.Title,
            renderedContent.Value.Body,
            request.BusinessIdempotencyKey);

        switch (request.Channel)
        {
            case NotificationChannelType.SiteMessage:
                await HandleSiteMessageAsync(dispatch, recipient.UserId!.Value, cancellationToken);
                break;
            case NotificationChannelType.Sms:
            case NotificationChannelType.MiniProgram:
                await HandlePlaceholderChannelAsync(dispatch, recipientProfileResult, cancellationToken);
                break;
            default:
                dispatch.MarkFailed($"Channel '{request.Channel}' is not supported.");
                break;
        }

        await _notificationDispatchCommandRepository.AddRangeAsync([dispatch], cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new SendNotificationResponse(
            Guid.NewGuid(),
            new NotificationDispatchResponse(
                dispatch.Id,
                dispatch.ChannelType,
                dispatch.Status,
                dispatch.FailureReason)));
    }

    private async Task HandleSiteMessageAsync(
        NotificationDispatch dispatch,
        Guid recipientUserId,
        CancellationToken cancellationToken)
    {
        var siteMessage = SiteMessage.Create(
            Guid.NewGuid(),
            dispatch.Id,
            recipientUserId,
            dispatch.Title,
            dispatch.Body);

        await _siteMessageCommandRepository.AddAsync(siteMessage, cancellationToken);
        dispatch.MarkDelivered();
    }

    private async Task HandlePlaceholderChannelAsync(
        NotificationDispatch dispatch,
        Result<RecipientProfileDto>? recipientProfileResult,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dispatch.RecipientAddressSnapshot))
        {
            if (recipientProfileResult is null)
            {
                dispatch.MarkFailed($"{dispatch.ChannelType} recipient profile lookup was not attempted.");
                return;
            }

            if (recipientProfileResult.IsFailure)
            {
                dispatch.MarkFailed(recipientProfileResult.Error.Message);
                return;
            }

            dispatch.MarkFailed($"{dispatch.ChannelType} recipient address is missing.");
            return;
        }

        if (!_channelSenders.TryGetValue(dispatch.ChannelType, out var sender))
        {
            dispatch.MarkFailed($"{dispatch.ChannelType} sender is not configured.");
            return;
        }

        var sendResult = await sender.SendAsync(
            new ChannelSendContext(
                dispatch.Id,
                dispatch.RecipientUserId,
                dispatch.RecipientAddressSnapshot,
                dispatch.ChannelType,
                dispatch.Title,
                dispatch.Body),
            cancellationToken);

        switch (sendResult.Status)
        {
            case NotificationDispatchStatus.Delivered:
                dispatch.MarkDelivered();
                break;
            case NotificationDispatchStatus.Placeholder:
                dispatch.MarkPlaceholder(sendResult.Message);
                break;
            case NotificationDispatchStatus.Failed:
                dispatch.MarkFailed(sendResult.Message ?? $"{dispatch.ChannelType} send failed.");
                break;
            default:
                dispatch.MarkPlaceholder(sendResult.Message);
                break;
        }
    }

    private static bool RequiresDirectoryLookup(NotificationChannelType channelType, NormalizedNotificationRecipient recipient)
    {
        return channelType switch
        {
            NotificationChannelType.Sms => string.IsNullOrWhiteSpace(recipient.PhoneNumber) && recipient.UserId.HasValue,
            NotificationChannelType.MiniProgram => string.IsNullOrWhiteSpace(recipient.MiniProgramOpenId) && recipient.UserId.HasValue,
            _ => false
        };
    }

    private static string? ResolveAddressSnapshot(
        NotificationChannelType channelType,
        NormalizedNotificationRecipient recipient,
        RecipientProfileDto? profile)
    {
        return channelType switch
        {
            NotificationChannelType.Sms => recipient.PhoneNumber ?? profile?.PhoneNumber,
            NotificationChannelType.MiniProgram => recipient.MiniProgramOpenId ?? profile?.MiniProgramOpenId,
            NotificationChannelType.SiteMessage => recipient.UserId?.ToString(),
            _ => null
        };
    }

    private static NormalizedNotificationRecipient NormalizeRecipient(SendNotificationRequest request)
    {
        if (request.Recipient is null)
        {
            return new NormalizedNotificationRecipient(
                request.RecipientUserId == Guid.Empty ? null : request.RecipientUserId,
                null,
                null);
        }

        return new NormalizedNotificationRecipient(
            request.Recipient.UserId ?? (request.RecipientUserId == Guid.Empty ? null : request.RecipientUserId),
            NormalizeOptional(request.Recipient.PhoneNumber),
            NormalizeOptional(request.Recipient.MiniProgramOpenId));
    }

    private static Error? ValidateRecipientCompatibility(
        NotificationChannelType channel,
        NormalizedNotificationRecipient recipient)
    {
        if (channel == NotificationChannelType.SiteMessage &&
            !recipient.UserId.HasValue)
        {
            return Error.Validation("Recipient.UserId is required for SiteMessage.");
        }

        return null;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record NormalizedNotificationRecipient(
        Guid? UserId,
        string? PhoneNumber,
        string? MiniProgramOpenId);
}

