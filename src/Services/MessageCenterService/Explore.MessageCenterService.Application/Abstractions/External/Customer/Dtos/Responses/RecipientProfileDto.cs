namespace Explore.MessageCenterService.Application.Abstractions.External;

public sealed record RecipientProfileDto(
    Guid UserId,
    string? PhoneNumber,
    string? MiniProgramOpenId);

