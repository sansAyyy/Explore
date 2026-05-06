namespace Explore.MessageCenterService.Application.Features.SiteMessages.Dtos.Responses;

public sealed record SiteMessageDetailResponse(
    Guid Id,
    Guid DispatchId,
    Guid UserId,
    string? Title,
    string Content,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);

