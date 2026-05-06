namespace Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;

public sealed record SiteMessageBasicResponse(
    Guid Id,
    Guid UserId,
    string? Title,
    string ContentPreview,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);

