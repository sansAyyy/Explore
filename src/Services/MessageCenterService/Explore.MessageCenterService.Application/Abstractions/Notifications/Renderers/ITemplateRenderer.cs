using BuildingBlocks.Common.Results;

namespace Explore.MessageCenterService.Application.Abstractions.Notifications;

public interface ITemplateRenderer
{
    Result<RenderedTemplateResult> Render(
        string? titleTemplate,
        string bodyTemplate,
        IReadOnlyDictionary<string, string> parameters);
}

