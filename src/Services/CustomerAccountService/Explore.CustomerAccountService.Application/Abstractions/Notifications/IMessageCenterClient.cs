using BuildingBlocks.Common.Results;

namespace Explore.CustomerAccountService.Application.Abstractions.Notifications;

public interface IMessageCenterClient
{
    Task<Result> SendPhoneLoginCodeAsync(
        string phoneNumber,
        string verificationCode,
        TimeSpan expiresIn,
        CancellationToken cancellationToken);
}

