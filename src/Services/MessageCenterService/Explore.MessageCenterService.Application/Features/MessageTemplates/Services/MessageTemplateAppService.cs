using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.Domain.Exceptions;
using Explore.MessageCenterService.Application.Abstractions.Persistence;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Abstractions;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Responses;
using Explore.MessageCenterService.Application.Features.MessageTemplates.Validators;
using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Features.MessageTemplates.Services;

public sealed class MessageTemplateAppService : IMessageTemplateAppService, IScopeDependency
{
    private readonly IMessageTemplateCommandRepository _commandRepository;
    private readonly IMessageTemplateQueryRepository _queryRepository;
    private readonly IMessageCenterUnitOfWork _unitOfWork;

    public MessageTemplateAppService(
        IMessageTemplateCommandRepository commandRepository,
        IMessageTemplateQueryRepository queryRepository,
        IMessageCenterUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<MessageTemplateBasicResponse>>> GetPagedAsync(
        GetPagedMessageTemplatesRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = MessageTemplateRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<PagedResult<MessageTemplateBasicResponse>>(validationError);
        }

        var pagedResult = await _queryRepository.GetPagedAsync(request, cancellationToken);
        return Result.Success(pagedResult);
    }

    public async Task<Result<MessageTemplateDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await _queryRepository.GetByIdAsync(id, cancellationToken);
        return template is null
            ? Result.Failure<MessageTemplateDetailResponse>(Error.NotFound($"Message template '{id}' was not found."))
            : Result.Success(template);
    }

    public async Task<Result<MessageTemplateDetailResponse>> CreateAsync(
        CreateMessageTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = MessageTemplateRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<MessageTemplateDetailResponse>(validationError);
        }

        var normalizedCode = request.Code.Trim().ToLowerInvariant();
        if (await _commandRepository.ExistsByCodeAsync(normalizedCode, null, cancellationToken))
        {
            return Result.Failure<MessageTemplateDetailResponse>(Error.Conflict($"Code '{normalizedCode}' already exists."));
        }

        try
        {
            var template = MessageTemplate.Create(
                Guid.NewGuid(),
                request.Code,
                request.Name,
                request.Description,
                request.IsEnabled,
                request.ChannelType,
                request.TitleTemplate,
                request.BodyTemplate);

            await _commandRepository.AddAsync(template, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return await LoadDetailAsync(template.Id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<MessageTemplateDetailResponse>(Error.Validation(exception.Message));
        }
    }

    public async Task<Result<MessageTemplateDetailResponse>> UpdateAsync(
        Guid id,
        UpdateMessageTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = MessageTemplateRequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result.Failure<MessageTemplateDetailResponse>(validationError);
        }

        var template = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (template is null)
        {
            return Result.Failure<MessageTemplateDetailResponse>(Error.NotFound($"Message template '{id}' was not found."));
        }

        var normalizedCode = request.Code.Trim().ToLowerInvariant();
        if (await _commandRepository.ExistsByCodeAsync(normalizedCode, id, cancellationToken))
        {
            return Result.Failure<MessageTemplateDetailResponse>(Error.Conflict($"Code '{normalizedCode}' already exists."));
        }

        try
        {
            template.Update(
                request.Code,
                request.Name,
                request.Description,
                request.IsEnabled,
                request.ChannelType,
                request.TitleTemplate,
                request.BodyTemplate);

            await _unitOfWork.CommitAsync(cancellationToken);
            return await LoadDetailAsync(template.Id, cancellationToken);
        }
        catch (DomainException exception)
        {
            return Result.Failure<MessageTemplateDetailResponse>(Error.Validation(exception.Message));
        }
    }

    public Task<Result> EnableAsync(Guid id, CancellationToken cancellationToken)
    {
        return ChangeActivationAsync(id, true, cancellationToken);
    }

    public Task<Result> DisableAsync(Guid id, CancellationToken cancellationToken)
    {
        return ChangeActivationAsync(id, false, cancellationToken);
    }

    private async Task<Result> ChangeActivationAsync(Guid id, bool isEnabled, CancellationToken cancellationToken)
    {
        var template = await _commandRepository.GetByIdAsync(id, cancellationToken);
        if (template is null)
        {
            return Result.Failure(Error.NotFound($"Message template '{id}' was not found."));
        }

        if (template.IsEnabled == isEnabled)
        {
            return Result.Success();
        }

        if (isEnabled)
        {
            template.Enable();
        }
        else
        {
            template.Disable();
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<MessageTemplateDetailResponse>> LoadDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await _queryRepository.GetByIdAsync(id, cancellationToken);
        return template is null
            ? Result.Failure<MessageTemplateDetailResponse>(Error.NotFound($"Message template '{id}' was not found."))
            : Result.Success(template);
    }
}

