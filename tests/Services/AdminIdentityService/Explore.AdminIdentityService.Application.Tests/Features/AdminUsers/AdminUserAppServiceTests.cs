using BuildingBlocks.Common.Pagination;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Services;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Responses;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Services;
using Explore.AdminIdentityService.Domain.AdminUsers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Explore.AdminIdentityService.Application.Tests.Features.AdminUsers;

public sealed class AdminUserAppServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldSendSiteMessage_ToCreatedAdminUser()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.CreateAsync(new CreateAdminUserRequest
        {
            UserName = "ops.admin",
            Email = "ops.admin@explore.local",
            DisplayName = "Operations Admin",
            Password = "ExamplePass@123",
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var notification = Assert.Single(fixture.SiteMessageSender.Requests);
        Assert.Equal(AdminIdentitySiteMessageTemplateCodes.AdminUserCreated, notification.TemplateCode);
        Assert.Equal(result.Value!.Id, notification.RecipientUserId);
        Assert.Equal("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", notification.Parameters["operatorAdminUserId"]);
        Assert.Equal("ops.admin@explore.local", notification.Parameters["email"]);
    }

    [Fact]
    public async Task ActivateAsync_ShouldSendSiteMessage_WhenStateActuallyChanges()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.CommandRepository.AddUser(
            "disabled.user",
            "disabled.user@explore.local",
            "Disabled User",
            "ExamplePass@123",
            isActive: false);

        var result = await fixture.Service.ActivateAsync(adminUser.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var notification = Assert.Single(fixture.SiteMessageSender.Requests);
        Assert.Equal(AdminIdentitySiteMessageTemplateCodes.AdminUserActivated, notification.TemplateCode);
        Assert.Equal(adminUser.Id, notification.RecipientUserId);
        Assert.Equal("enabled", notification.Parameters["status"]);
    }

    [Fact]
    public async Task ActivateAsync_ShouldNotSendSiteMessage_WhenStateIsAlreadyActive()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.CommandRepository.AddUser(
            "active.user",
            "active.user@explore.local",
            "Active User",
            "ExamplePass@123",
            isActive: true);

        var result = await fixture.Service.ActivateAsync(adminUser.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(fixture.SiteMessageSender.Requests);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldSendSiteMessage_WhenStateActuallyChanges()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.CommandRepository.AddUser(
            "active.user",
            "active.user@explore.local",
            "Active User",
            "ExamplePass@123",
            isActive: true);

        var result = await fixture.Service.DeactivateAsync(adminUser.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var notification = Assert.Single(fixture.SiteMessageSender.Requests);
        Assert.Equal(AdminIdentitySiteMessageTemplateCodes.AdminUserDeactivated, notification.TemplateCode);
        Assert.Equal("disabled", notification.Parameters["status"]);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldNotSendSiteMessage_WhenStateIsAlreadyInactive()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.CommandRepository.AddUser(
            "disabled.user",
            "disabled.user@explore.local",
            "Disabled User",
            "ExamplePass@123",
            isActive: false);

        var result = await fixture.Service.DeactivateAsync(adminUser.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(fixture.SiteMessageSender.Requests);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldSendSiteMessage_WhenPasswordChangedByAdmin()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.CommandRepository.AddUser(
            "target.user",
            "target.user@explore.local",
            "Target User",
            "ExamplePass@123");

        var result = await fixture.Service.ChangePasswordAsync(adminUser.Id, new ChangeAdminUserPasswordRequest
        {
            NewPassword = "ExamplePass@456"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var notification = Assert.Single(fixture.SiteMessageSender.Requests);
        Assert.Equal(AdminIdentitySiteMessageTemplateCodes.AdminUserPasswordChangedByAdmin, notification.TemplateCode);
        Assert.Equal(adminUser.Id, notification.RecipientUserId);
        Assert.True(notification.Parameters.ContainsKey("changedAt"));
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldNotSendSiteMessage_WhenPasswordChangeFails()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.CommandRepository.AddUser(
            "target.user",
            "target.user@explore.local",
            "Target User",
            "ExamplePass@123");

        var result = await fixture.Service.ChangePasswordAsync(adminUser.Id, new ChangeAdminUserPasswordRequest
        {
            NewPassword = "ExamplePass@123"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Empty(fixture.SiteMessageSender.Requests);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenSiteMessageSendingFails()
    {
        var fixture = CreateFixture(senderResult: Result.Failure(Error.BadRequest("Message center unavailable.")));

        var result = await fixture.Service.CreateAsync(new CreateAdminUserRequest
        {
            UserName = "ops.admin",
            Email = "ops.admin@explore.local",
            DisplayName = "Operations Admin",
            Password = "ExamplePass@123",
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Single(fixture.SiteMessageSender.Requests);
    }

    private static Fixture CreateFixture(Result? senderResult = null)
    {
        var commandRepository = new FakeAdminUserCommandRepository();
        var queryRepository = new FakeAdminUserQueryRepository(commandRepository);
        var siteMessageSender = new FakeAdminSiteMessageSender(senderResult ?? Result.Success());
        var currentUser = new FakeCurrentUser { UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") };
        var unitOfWork = new FakeAdminIdentityUnitOfWork();
        var service = new AdminUserAppService(
            commandRepository,
            queryRepository,
            new PasswordHashService(),
            currentUser,
            siteMessageSender,
            NullLogger<AdminUserAppService>.Instance,
            unitOfWork);

        return new Fixture(service, commandRepository, siteMessageSender, unitOfWork);
    }

    private sealed record Fixture(
        AdminUserAppService Service,
        FakeAdminUserCommandRepository CommandRepository,
        FakeAdminSiteMessageSender SiteMessageSender,
        FakeAdminIdentityUnitOfWork UnitOfWork);

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }

        public string AuditActor => UserId?.ToString() ?? "Test";
    }

    private sealed class FakeAdminSiteMessageSender : IAdminSiteMessageSender
    {
        private readonly Result _result;

        public FakeAdminSiteMessageSender(Result result)
        {
            _result = result;
        }

        public List<AdminSiteMessageRequest> Requests { get; } = [];

        public Task<Result> SendAsync(AdminSiteMessageRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeAdminUserCommandRepository : IAdminUserCommandRepository
    {
        private readonly List<AdminUser> _users = [];
        private readonly PasswordHashService _passwordHashService = new();

        public IReadOnlyList<AdminUser> Users => _users;

        public AdminUser AddUser(
            string userName,
            string email,
            string displayName,
            string password,
            string? phoneNumber = null,
            bool isActive = true)
        {
            var user = AdminUser.Create(
                Guid.NewGuid(),
                userName,
                email,
                displayName,
                phoneNumber,
                _passwordHashService.HashPassword(password),
                isActive);

            _users.Add(user);
            return user;
        }

        public Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.SingleOrDefault(x => x.Id == id));
        }

        public Task<AdminUser?> GetByAccountAsync(string account, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.SingleOrDefault(x => x.UserName == account || x.Email == account));
        }

        public Task<AdminUser?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.SingleOrDefault(x => x.PhoneNumber == phoneNumber));
        }

        public Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken)
        {
            _users.Add(adminUser);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludedId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.Any(x => x.UserName == userName && (!excludedId.HasValue || x.Id != excludedId.Value)));
        }

        public Task<bool> ExistsByEmailAsync(string email, Guid? excludedId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.Any(x => x.Email == email && (!excludedId.HasValue || x.Id != excludedId.Value)));
        }

        public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, Guid? excludedId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.Any(x => x.PhoneNumber == phoneNumber && (!excludedId.HasValue || x.Id != excludedId.Value)));
        }

        public void Remove(AdminUser adminUser)
        {
            _users.Remove(adminUser);
        }

    }

    private sealed class FakeAdminIdentityUnitOfWork : IAdminIdentityUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            CommitCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeAdminUserQueryRepository : IAdminUserQueryRepository
    {
        private readonly FakeAdminUserCommandRepository _commandRepository;

        public FakeAdminUserQueryRepository(FakeAdminUserCommandRepository commandRepository)
        {
            _commandRepository = commandRepository;
        }

        public Task<AdminUserDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = _commandRepository.Users.SingleOrDefault(x => x.Id == id);
            return Task.FromResult(user is null
                ? null
                : new AdminUserDetailResponse(
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user.DisplayName,
                    user.IsActive,
                    user.CreatedAt,
                    user.CreatedBy,
                    user.UpdatedAt,
                    user.UpdatedBy,
                    user.LastLoginAt,
                    user.Version));
        }

        public Task<PagedResult<AdminUserBasicResponse>> GetPagedAsync(GetPagedAdminUsersRequest request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}

