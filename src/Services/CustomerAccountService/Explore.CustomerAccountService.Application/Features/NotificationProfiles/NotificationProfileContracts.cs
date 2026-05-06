namespace Explore.CustomerAccountService.Application.Features.NotificationProfiles;

public interface INotificationProfileAppService
{
    Task<BuildingBlocks.Common.Results.Result<CustomerNotificationProfileResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}

public sealed record CustomerNotificationProfileResponse(
    Guid UserId,
    string PhoneNumber,
    string? MiniProgramOpenId);

