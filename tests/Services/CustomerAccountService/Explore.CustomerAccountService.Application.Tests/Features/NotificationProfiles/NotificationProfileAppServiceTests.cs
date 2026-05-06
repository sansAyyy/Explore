using BuildingBlocks.Common.Results;
using Explore.CustomerAccountService.Application.Features.NotificationProfiles;
using Explore.CustomerAccountService.Application.Tests.Testing;

namespace Explore.CustomerAccountService.Application.Tests.Features.NotificationProfiles;

public sealed class NotificationProfileAppServiceTests
{
    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnProfile()
    {
        var repository = new FakeCustomerCommandRepository();
        var customer = repository.AddCustomer("13800138000", "Alice");
        var service = new NotificationProfileAppService(repository);

        var result = await service.GetByUserIdAsync(customer.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customer.Id, result.Value!.UserId);
        Assert.Equal("13800138000", result.Value.PhoneNumber);
        Assert.Null(result.Value.MiniProgramOpenId);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNotFound()
    {
        var service = new NotificationProfileAppService(new FakeCustomerCommandRepository());

        var result = await service.GetByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
    }
}

