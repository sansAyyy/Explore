using BuildingBlocks.Common.Results;

namespace Explore.AdminIdentityService.Application.Abstractions.Notifications;

public interface IAdminMessageCenterClient
{
    Task<Result> SendPhoneLoginCodeAsync(
        string phoneNumber,
        string verificationCode,
        TimeSpan expiresIn,
        CancellationToken cancellationToken);
}

