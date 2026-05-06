using Explore.MessageCenterService.Domain.MessageTemplates;

namespace Explore.MessageCenterService.Application.Features.MessageTemplates.Dtos.Requests;

public sealed class CreateMessageTemplateRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;

    public NotificationChannelType ChannelType { get; set; }

    public string? TitleTemplate { get; set; }

    public string BodyTemplate { get; set; } = string.Empty;
}

