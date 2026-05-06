using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Services;
using Explore.CustomerAccountService.Application.Tests.Testing;

namespace Explore.CustomerAccountService.Application.Tests.Features.AdminCustomers;

public sealed class AdminCustomerAppServiceTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedCustomers()
    {
        var commandRepository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var queryRepository = new FakeAdminCustomerQueryRepository
        {
            PagedResult = new PagedResult<AdminCustomerBasicResponse>(
                1,
                [
                    new AdminCustomerBasicResponse(
                        Guid.NewGuid(),
                        "13800138000",
                        "Alice",
                        null,
                        "alice@explore.local",
                        true,
                        DateTime.UtcNow,
                        null,
                        null)
                ])
        };
        var service = new AdminCustomerAppService(commandRepository, queryRepository, unitOfWork);

        var result = await service.GetPagedAsync(new GetPagedAdminCustomersRequest
        {
            PageIndex = 1,
            PageSize = 10,
            Keyword = "Alice",
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal("Alice", result.Value.Items.Single().NickName);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnValidationError_WhenRequestIsInvalid()
    {
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var service = new AdminCustomerAppService(
            new FakeCustomerCommandRepository(),
            new FakeAdminCustomerQueryRepository(),
            unitOfWork);

        var result = await service.GetPagedAsync(new GetPagedAdminCustomersRequest
        {
            PageIndex = 0,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ValidationFailed, result.Error.Code);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDetail_WhenCustomerExists()
    {
        var customerId = Guid.NewGuid();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var service = new AdminCustomerAppService(
            new FakeCustomerCommandRepository(),
            new FakeAdminCustomerQueryRepository
            {
                DetailResponse = new AdminCustomerDetailResponse(
                    customerId,
                    "13800138000",
                    "Alice",
                    "https://example.com/avatar.png",
                    "alice@explore.local",
                    true,
                    DateTime.UtcNow,
                    "system",
                    DateTime.UtcNow,
                    "system",
                    DateTime.UtcNow,
                    1)
            },
            unitOfWork);

        var result = await service.GetByIdAsync(customerId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customerId, result.Value!.Id);
        Assert.Equal("Alice", result.Value.NickName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var service = new AdminCustomerAppService(
            new FakeCustomerCommandRepository(),
            new FakeAdminCustomerQueryRepository(),
            unitOfWork);

        var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
    }

    [Fact]
    public async Task ActivateAsync_ShouldActivateInactiveCustomer()
    {
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var customer = repository.AddCustomer("13800138000", "Alice", isActive: false);
        var service = new AdminCustomerAppService(repository, new FakeAdminCustomerQueryRepository(), unitOfWork);

        var result = await service.ActivateAsync(customer.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(repository.Customers.Single(x => x.Id == customer.Id).IsActive);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivateActiveCustomer()
    {
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var customer = repository.AddCustomer("13800138000", "Alice", isActive: true);
        var service = new AdminCustomerAppService(repository, new FakeAdminCustomerQueryRepository(), unitOfWork);

        var result = await service.DeactivateAsync(customer.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(repository.Customers.Single(x => x.Id == customer.Id).IsActive);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task ActivateAsync_ShouldBeIdempotent_WhenCustomerAlreadyActive()
    {
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var customer = repository.AddCustomer("13800138000", "Alice", isActive: true);
        var service = new AdminCustomerAppService(repository, new FakeAdminCustomerQueryRepository(), unitOfWork);

        var result = await service.ActivateAsync(customer.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(repository.Customers.Single(x => x.Id == customer.Id).IsActive);
        Assert.Equal(0, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldBeIdempotent_WhenCustomerAlreadyInactive()
    {
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var customer = repository.AddCustomer("13800138000", "Alice", isActive: false);
        var service = new AdminCustomerAppService(repository, new FakeAdminCustomerQueryRepository(), unitOfWork);

        var result = await service.DeactivateAsync(customer.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(repository.Customers.Single(x => x.Id == customer.Id).IsActive);
        Assert.Equal(0, unitOfWork.CommitCount);
    }
}

