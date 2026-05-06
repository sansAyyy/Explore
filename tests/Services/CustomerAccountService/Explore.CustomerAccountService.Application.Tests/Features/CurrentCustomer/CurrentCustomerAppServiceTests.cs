using Explore.CustomerAccountService.Application.Features.CurrentCustomer;
using Explore.CustomerAccountService.Application.Tests.Testing;

namespace Explore.CustomerAccountService.Application.Tests.Features.CurrentCustomer;

public sealed class CurrentCustomerAppServiceTests
{
    [Fact]
    public async Task GetCurrentAsync_ShouldReturnCurrentCustomer()
    {
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var currentUser = new FakeCurrentUser();
        var customer = repository.AddCustomer("13800138000", "Alice");
        currentUser.UserId = customer.Id;
        var service = new CurrentCustomerAppService(currentUser, repository, unitOfWork);

        var result = await service.GetCurrentAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customer.Id, result.Value!.Id);
        Assert.Equal("Alice", result.Value.NickName);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateProfile()
    {
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var currentUser = new FakeCurrentUser();
        var customer = repository.AddCustomer("13800138000", "Alice");
        currentUser.UserId = customer.Id;
        var service = new CurrentCustomerAppService(currentUser, repository, unitOfWork);

        var result = await service.UpdateProfileAsync(new UpdateCurrentCustomerProfileRequest
        {
            NickName = "Alice Updated",
            AvatarUrl = "https://example.com/a.png",
            Email = "alice@explore.local"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Alice Updated", result.Value!.NickName);
        Assert.Equal("alice@explore.local", result.Value.Email);
        Assert.Equal("https://example.com/a.png", result.Value.AvatarUrl);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldReturnConflict_WhenEmailExists()
    {
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        repository.AddCustomer("13800138001", "First", email: "used@explore.local");
        var currentUser = new FakeCurrentUser();
        var customer = repository.AddCustomer("13800138000", "Second");
        currentUser.UserId = customer.Id;
        var service = new CurrentCustomerAppService(currentUser, repository, unitOfWork);

        var result = await service.UpdateProfileAsync(new UpdateCurrentCustomerProfileRequest
        {
            NickName = "Second Updated",
            Email = "used@explore.local"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BuildingBlocks.Common.Results.ErrorCodes.Conflict, result.Error.Code);
        Assert.Equal(0, unitOfWork.CommitCount);
    }
}

