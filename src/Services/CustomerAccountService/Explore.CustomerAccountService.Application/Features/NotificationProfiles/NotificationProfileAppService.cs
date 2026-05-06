using BuildingBlocks.Common.Results;
using BuildingBlocks.DependencyInjection.Abstractions;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;

namespace Explore.CustomerAccountService.Application.Features.NotificationProfiles;

public sealed class NotificationProfileAppService : INotificationProfileAppService, IScopeDependency
{
    private readonly ICustomerCommandRepository _commandRepository;

    public NotificationProfileAppService(ICustomerCommandRepository commandRepository)
    {
        _commandRepository = commandRepository;
    }

    public async Task<Result<CustomerNotificationProfileResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var customer = await _commandRepository.GetByIdAsync(userId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<CustomerNotificationProfileResponse>(Error.NotFound($"Customer '{userId}' was not found."));
        }

        return Result.Success(new CustomerNotificationProfileResponse(
            customer.Id,
            customer.PhoneNumber,
            null));
    }
}

