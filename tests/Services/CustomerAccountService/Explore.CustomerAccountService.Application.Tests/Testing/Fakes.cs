using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.Pagination;
using BuildingBlocks.CurrentUser.Abstractions;
using Explore.CustomerAccountService.Application.Abstractions.Notifications;
using Explore.CustomerAccountService.Application.Abstractions.Persistence;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Requests;
using Explore.CustomerAccountService.Application.Features.AdminCustomers.Dtos.Responses;
using Explore.CustomerAccountService.Domain.Customers;

namespace Explore.CustomerAccountService.Application.Tests.Testing;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public Guid? UserId { get; set; }

    public string AuditActor => UserId?.ToString() ?? "Test";
}

internal sealed class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);

    public object? GetRawValue(string key)
    {
        return TryGetEntry(key, out var entry) ? entry.Value : null;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (!TryGetEntry(key, out var entry))
        {
            return Task.FromResult(default(T));
        }

        return Task.FromResult((T?)entry.Value);
    }

    public Task SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        _entries[key] = new CacheEntry(value, null);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        _entries[key] = new CacheEntry(value, DateTime.UtcNow.Add(ttl));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _entries.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return Task.FromResult(TryGetEntry(key, out _));
    }

    private bool TryGetEntry(string key, out CacheEntry entry)
    {
        if (_entries.TryGetValue(key, out entry!) &&
            (!entry.ExpiresAt.HasValue || entry.ExpiresAt.Value > DateTime.UtcNow))
        {
            return true;
        }

        _entries.Remove(key);
        entry = default!;
        return false;
    }

    private sealed record CacheEntry(object? Value, DateTime? ExpiresAt);
}

internal sealed class FakeCustomerCommandRepository : ICustomerCommandRepository
{
    private readonly List<Customer> _customers = [];

    public IReadOnlyList<Customer> Customers => _customers;

    public Customer AddCustomer(
        string phoneNumber,
        string nickName = "Test Customer",
        string? avatarUrl = null,
        string? email = null,
        bool isActive = true)
    {
        var customer = Customer.Create(Guid.NewGuid(), phoneNumber, nickName, avatarUrl, email, isActive);
        _customers.Add(customer);
        return customer;
    }

    public void SetCustomerActive(Guid customerId, bool isActive)
    {
        var customer = _customers.Single(x => x.Id == customerId);
        if (isActive)
        {
            customer.Activate();
            return;
        }

        customer.Deactivate();
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_customers.SingleOrDefault(x => x.Id == id));
    }

    public Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        return Task.FromResult(_customers.SingleOrDefault(x => x.PhoneNumber == phoneNumber));
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_customers.Any(x => x.Email == email && (!excludedId.HasValue || x.Id != excludedId.Value)));
    }
}

internal sealed class FakeCustomerAccountUnitOfWork : ICustomerAccountUnitOfWork
{
    public int CommitCount { get; private set; }

    public Func<CancellationToken, Task<int>>? OnCommitAsync { get; set; }

    public Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        CommitCount++;
        return OnCommitAsync?.Invoke(cancellationToken) ?? Task.FromResult(1);
    }
}

internal sealed class FakeAdminCustomerQueryRepository : IAdminCustomerQueryRepository
{
    public AdminCustomerDetailResponse? DetailResponse { get; set; }

    public PagedResult<AdminCustomerBasicResponse> PagedResult { get; set; } =
        new(0, Array.Empty<AdminCustomerBasicResponse>());

    public Task<AdminCustomerDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(DetailResponse);
    }

    public Task<PagedResult<AdminCustomerBasicResponse>> GetPagedAsync(
        GetPagedAdminCustomersRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(PagedResult);
    }
}

internal sealed class FakeMessageCenterClient : IMessageCenterClient
{
    public Result NextResult { get; set; } = Result.Success();

    public List<(string PhoneNumber, string VerificationCode, TimeSpan ExpiresIn)> Requests { get; } = [];

    public Task<Result> SendPhoneLoginCodeAsync(
        string phoneNumber,
        string verificationCode,
        TimeSpan expiresIn,
        CancellationToken cancellationToken)
    {
        Requests.Add((phoneNumber, verificationCode, expiresIn));
        return Task.FromResult(NextResult);
    }
}

