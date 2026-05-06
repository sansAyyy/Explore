using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.Security.Authentication.Constants;
using BuildingBlocks.Security.Authentication.Options;
using BuildingBlocks.Security.Authentication.Jwt.Services;
using BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Services;
using BuildingBlocks.Security.PhoneVerification.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Options;
using BuildingBlocks.Security.PhoneVerification.Services;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.AdminIdentityService.Application.Abstractions.Persistence;
using Explore.AdminIdentityService.Application.Features.Authentication.Abstractions;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.Authentication.Services;
using Explore.AdminIdentityService.Domain.AdminUsers;
using Microsoft.Extensions.Options;

namespace Explore.AdminIdentityService.Application.Tests.Features.Authentication;

public sealed class AdminAuthenticationAppServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldReturnAccessAndRefreshTokens()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.Repository.AddUser(
            userName: "seed-admin",
            email: "seed-admin@example.test",
            displayName: "Platform Super Admin",
            password: "ExamplePass@123");

        var result = await fixture.Service.LoginAsync(new AdminLoginRequest
        {
            Account = "seed-admin",
            Password = "ExamplePass@123"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
        Assert.Equal("Bearer", result.Value.TokenType);
        Assert.True(result.Value.RefreshTokenExpiresAt > result.Value.ExpiresAt);
        Assert.Equal(TokenParties.Admin, ReadTokenParty(result.Value.AccessToken));

        var refreshValidation = await fixture.RefreshTokenService.ValidateAsync(result.Value.RefreshToken, CancellationToken.None);
        Assert.True(refreshValidation.IsSuccess);
        Assert.Equal(adminUser.Id, refreshValidation.Value!.UserId);
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldSendCodeForExistingActiveAdmin()
    {
        var fixture = CreateFixture();
        fixture.Repository.AddUser(
            userName: "ops.admin",
            email: "ops.admin@explore.local",
            displayName: "Operations Admin",
            password: "ExamplePass@123",
            phoneNumber: "13800138000");

        var result = await fixture.Service.SendPhoneLoginCodeAsync(new AdminSendPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var request = Assert.Single(fixture.MessageCenterClient.Requests);
        Assert.Equal("13800138000", request.PhoneNumber);
        Assert.Matches("^[0-9]{6}$", request.VerificationCode);
        Assert.InRange(request.ExpiresIn, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(5));
        Assert.True(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:code:13800138000", CancellationToken.None));
        Assert.True(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:send-interval:13800138000", CancellationToken.None));

        var rawCacheValue = fixture.Cache.GetRawValue("admin-auth:phone-verification:code:13800138000");
        Assert.NotNull(rawCacheValue);
        Assert.DoesNotContain(request.VerificationCode, JsonSerializer.Serialize(rawCacheValue, rawCacheValue.GetType()));
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldReturnSuccess_WhenPhoneNumberIsNotBound()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.SendPhoneLoginCodeAsync(new AdminSendPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(fixture.MessageCenterClient.Requests);
        Assert.False(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:code:13800138000", CancellationToken.None));
        Assert.False(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:send-interval:13800138000", CancellationToken.None));
        Assert.Empty(fixture.Repository.Users);
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldReturnSuccess_WhenAdminUserIsDisabled()
    {
        var fixture = CreateFixture();
        fixture.Repository.AddUser(
            userName: "ops.admin",
            email: "ops.admin@explore.local",
            displayName: "Operations Admin",
            password: "ExamplePass@123",
            phoneNumber: "13800138000",
            isActive: false);

        var result = await fixture.Service.SendPhoneLoginCodeAsync(new AdminSendPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(fixture.MessageCenterClient.Requests);
        Assert.False(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:code:13800138000", CancellationToken.None));
        Assert.False(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:send-interval:13800138000", CancellationToken.None));
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldNotCacheCode_WhenMessageCenterFails()
    {
        var fixture = CreateFixture();
        fixture.Repository.AddUser(
            userName: "ops.admin",
            email: "ops.admin@explore.local",
            displayName: "Operations Admin",
            password: "ExamplePass@123",
            phoneNumber: "13800138000");
        fixture.MessageCenterClient.NextResult = Result.Failure(Error.BadRequest("Message center unavailable."));

        var result = await fixture.Service.SendPhoneLoginCodeAsync(new AdminSendPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Single(fixture.MessageCenterClient.Requests);
        Assert.False(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:code:13800138000", CancellationToken.None));
        Assert.False(await fixture.Cache.ExistsAsync("admin-auth:phone-verification:send-interval:13800138000", CancellationToken.None));
    }

    [Fact]
    public async Task PhoneLoginAsync_ShouldReturnFailure_WhenPhoneNumberIsNotBound()
    {
        var fixture = CreateFixture();
        const string phoneNumber = "13800138000";
        var verificationCode = await IssueCodeAsync(fixture, phoneNumber);

        var result = await fixture.Service.PhoneLoginAsync(new AdminPhoneLoginRequest
        {
            PhoneNumber = phoneNumber,
            VerificationCode = verificationCode
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.BadRequest, result.Error.Code);
        Assert.Equal("Phone number is not bound to any admin user.", result.Error.Message);
        Assert.Empty(fixture.Repository.Users);
    }

    [Fact]
    public async Task PhoneLoginAsync_ShouldLoginExistingAdmin()
    {
        var fixture = CreateFixture();
        const string phoneNumber = "13800138000";
        fixture.Repository.AddUser(
            userName: "ops.admin",
            email: "ops.admin@explore.local",
            displayName: "Operations Admin",
            password: "ExamplePass@123",
            phoneNumber: phoneNumber);
        var verificationCode = await IssueCodeAsync(fixture, phoneNumber);

        var result = await fixture.Service.PhoneLoginAsync(new AdminPhoneLoginRequest
        {
            PhoneNumber = phoneNumber,
            VerificationCode = verificationCode
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(fixture.Repository.Users);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.RefreshToken));
    }

    [Fact]
    public async Task PhoneLoginAsync_ShouldReturnForbidden_WhenAdminUserIsDisabled()
    {
        var fixture = CreateFixture();
        const string phoneNumber = "13800138000";
        fixture.Repository.AddUser(
            userName: "ops.admin",
            email: "ops.admin@explore.local",
            displayName: "Operations Admin",
            password: "ExamplePass@123",
            phoneNumber: phoneNumber,
            isActive: false);
        var verificationCode = await IssueCodeAsync(fixture, phoneNumber);

        var result = await fixture.Service.PhoneLoginAsync(new AdminPhoneLoginRequest
        {
            PhoneNumber = phoneNumber,
            VerificationCode = verificationCode
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Forbidden, result.Error.Code);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRotateRefreshToken()
    {
        var fixture = CreateFixture();
        fixture.Repository.AddUser(
            userName: "seed-admin",
            email: "seed-admin@example.test",
            displayName: "Platform Super Admin",
            password: "ExamplePass@123");

        var loginResult = await fixture.Service.LoginAsync(new AdminLoginRequest
        {
            Account = "seed-admin",
            Password = "ExamplePass@123"
        }, CancellationToken.None);

        Assert.True(loginResult.IsSuccess);

        var refreshResult = await fixture.Service.RefreshAsync(new AdminRefreshTokenRequest
        {
            RefreshToken = loginResult.Value!.RefreshToken
        }, CancellationToken.None);

        Assert.True(refreshResult.IsSuccess);
        Assert.NotNull(refreshResult.Value);
        Assert.NotEqual(loginResult.Value.RefreshToken, refreshResult.Value!.RefreshToken);
        Assert.Equal(TokenParties.Admin, ReadTokenParty(refreshResult.Value.AccessToken));

        var oldRefreshValidation = await fixture.RefreshTokenService.ValidateAsync(loginResult.Value.RefreshToken, CancellationToken.None);
        Assert.True(oldRefreshValidation.IsFailure);
        Assert.Equal(ErrorCodes.Unauthorized, oldRefreshValidation.Error.Code);

        var newRefreshValidation = await fixture.RefreshTokenService.ValidateAsync(refreshResult.Value.RefreshToken, CancellationToken.None);
        Assert.True(newRefreshValidation.IsSuccess);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnUnauthorized_WhenRefreshTokenIsMalformed()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.RefreshAsync(new AdminRefreshTokenRequest
        {
            RefreshToken = "malformed-token"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Unauthorized, result.Error.Code);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnForbidden_WhenUserIsDisabled()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.Repository.AddUser(
            userName: "seed-admin",
            email: "seed-admin@example.test",
            displayName: "Platform Super Admin",
            password: "ExamplePass@123");

        var loginResult = await fixture.Service.LoginAsync(new AdminLoginRequest
        {
            Account = "seed-admin",
            Password = "ExamplePass@123"
        }, CancellationToken.None);

        fixture.Repository.SetUserActive(adminUser.Id, false);

        var refreshResult = await fixture.Service.RefreshAsync(new AdminRefreshTokenRequest
        {
            RefreshToken = loginResult.Value!.RefreshToken
        }, CancellationToken.None);

        Assert.True(refreshResult.IsFailure);
        Assert.Equal(ErrorCodes.Forbidden, refreshResult.Error.Code);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeOnlyCurrentSession()
    {
        var fixture = CreateFixture();
        var adminUser = fixture.Repository.AddUser(
            userName: "seed-admin",
            email: "seed-admin@example.test",
            displayName: "Platform Super Admin",
            password: "ExamplePass@123");
        fixture.CurrentUser.UserId = adminUser.Id;

        var firstLogin = await fixture.Service.LoginAsync(new AdminLoginRequest
        {
            Account = "seed-admin",
            Password = "ExamplePass@123"
        }, CancellationToken.None);
        var secondLogin = await fixture.Service.LoginAsync(new AdminLoginRequest
        {
            Account = "seed-admin",
            Password = "ExamplePass@123"
        }, CancellationToken.None);

        var logoutResult = await fixture.Service.LogoutAsync(new AdminLogoutRequest
        {
            RefreshToken = firstLogin.Value!.RefreshToken
        }, CancellationToken.None);

        Assert.True(logoutResult.IsSuccess);

        var firstValidation = await fixture.RefreshTokenService.ValidateAsync(firstLogin.Value.RefreshToken, CancellationToken.None);
        var secondValidation = await fixture.RefreshTokenService.ValidateAsync(secondLogin.Value!.RefreshToken, CancellationToken.None);

        Assert.True(firstValidation.IsFailure);
        Assert.Equal(ErrorCodes.Unauthorized, firstValidation.Error.Code);
        Assert.True(secondValidation.IsSuccess);
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnForbidden_WhenRefreshTokenBelongsToAnotherUser()
    {
        var fixture = CreateFixture();
        var currentUser = fixture.Repository.AddUser(
            userName: "current",
            email: "current@explore.local",
            displayName: "Current User",
            password: "ExamplePass@123");
        var anotherUser = fixture.Repository.AddUser(
            userName: "another",
            email: "another@explore.local",
            displayName: "Another User",
            password: "ExamplePass@123");
        fixture.CurrentUser.UserId = currentUser.Id;

        var loginResult = await fixture.Service.LoginAsync(new AdminLoginRequest
        {
            Account = "another",
            Password = "ExamplePass@123"
        }, CancellationToken.None);

        var logoutResult = await fixture.Service.LogoutAsync(new AdminLogoutRequest
        {
            RefreshToken = loginResult.Value!.RefreshToken
        }, CancellationToken.None);

        Assert.True(logoutResult.IsFailure);
        Assert.Equal(ErrorCodes.Forbidden, logoutResult.Error.Code);

        var refreshValidation = await fixture.RefreshTokenService.ValidateAsync(loginResult.Value.RefreshToken, CancellationToken.None);
        Assert.True(refreshValidation.IsSuccess);
        Assert.Equal(anotherUser.Id, refreshValidation.Value!.UserId);
    }

    private static AuthenticationFixture CreateFixture()
    {
        var cache = new FakeCacheService();
        var repository = new FakeAdminUserCommandRepository();
        var currentUser = new FakeCurrentUser();
        var messageCenterClient = new FakeAdminMessageCenterClient();
        var unitOfWork = new FakeAdminIdentityUnitOfWork();
        var passwordHashService = new PasswordHashService();
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "Explore.Tests",
            Audience = "Explore.Tests.Clients",
            SigningKey = "Explore.Tests.DevSigningKey.ReplaceBeforeProduction.2026",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7
        });
        var verificationOptions = Options.Create(new PhoneVerificationCodeOptions());
        var phoneVerificationCodeService = new PhoneVerificationCodeService(cache, verificationOptions);
        var jwtTokenService = new JwtTokenService(jwtOptions);
        var refreshTokenService = new AdminRefreshTokenService(cache, jwtOptions);
        var authenticationService = new AdminAuthenticationAppService(
            repository,
            currentUser,
            jwtTokenService,
            messageCenterClient,
            refreshTokenService,
            phoneVerificationCodeService,
            passwordHashService,
            unitOfWork);

        return new AuthenticationFixture(
            authenticationService,
            refreshTokenService,
            phoneVerificationCodeService,
            repository,
            unitOfWork,
            cache,
            currentUser,
            messageCenterClient);
    }

    private static async Task<string> IssueCodeAsync(AuthenticationFixture fixture, string phoneNumber)
    {
        var issueResult = await fixture.PhoneVerificationCodeService.IssueAsync("admin-auth", phoneNumber, CancellationToken.None);
        Assert.True(issueResult.IsSuccess);
        return issueResult.Value!.Code;
    }

    private static string? ReadTokenParty(string accessToken)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        return token.Claims.SingleOrDefault(x => x.Type == SecurityClaimNames.TokenParty)?.Value;
    }

    private sealed record AuthenticationFixture(
        AdminAuthenticationAppService Service,
        IAdminRefreshTokenService RefreshTokenService,
        IPhoneVerificationCodeService PhoneVerificationCodeService,
        FakeAdminUserCommandRepository Repository,
        FakeAdminIdentityUnitOfWork UnitOfWork,
        FakeCacheService Cache,
        FakeCurrentUser CurrentUser,
        FakeAdminMessageCenterClient MessageCenterClient);

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }

        public string AuditActor => UserId?.ToString() ?? "Test";
    }

    private sealed class FakeCacheService : ICacheService
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

    private sealed class FakeAdminMessageCenterClient : IAdminMessageCenterClient
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

    private sealed class FakeAdminUserCommandRepository : IAdminUserCommandRepository
    {
        private readonly List<AdminUser> _users = [];

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
                new PasswordHashService().HashPassword(password),
                isActive);

            _users.Add(user);
            return user;
        }

        public void SetUserActive(Guid userId, bool isActive)
        {
            var user = _users.Single(x => x.Id == userId);
            if (isActive)
            {
                user.Activate();
                return;
            }

            user.Deactivate();
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
}

