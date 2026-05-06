using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Responses;

public sealed record MessageTemplateBasicResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsEnabled,
    NotificationChannelType ChannelType);

