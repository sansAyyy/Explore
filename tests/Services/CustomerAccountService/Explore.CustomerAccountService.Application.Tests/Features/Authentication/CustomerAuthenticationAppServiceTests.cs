using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Security.Authentication.Constants;
using BuildingBlocks.Security.Authentication.Options;
using BuildingBlocks.Security.Authentication.Jwt.Services;
using BuildingBlocks.Security.PhoneVerification.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Options;
using BuildingBlocks.Security.PhoneVerification.Services;
using Explore.CustomerAccountService.Application.Features.Authentication;
using Explore.CustomerAccountService.Application.Tests.Testing;
using Microsoft.Extensions.Options;

namespace Explore.CustomerAccountService.Application.Tests.Features.Authentication;

public sealed class CustomerAuthenticationAppServiceTests
{
    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldEnforceRateLimit()
    {
        var fixture = CreateFixture();

        var firstResult = await fixture.Service.SendPhoneLoginCodeAsync(new SendCustomerPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);
        var secondResult = await fixture.Service.SendPhoneLoginCodeAsync(new SendCustomerPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailure);
        Assert.Equal(ErrorCodes.BadRequest, secondResult.Error.Code);
        Assert.Single(fixture.MessageCenterClient.Requests);
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldNotLeaveVerifiableCode_WhenMessageCenterFails()
    {
        var fixture = CreateFixture();
        fixture.MessageCenterClient.NextResult = Result.Failure(Error.BadRequest("Message center unavailable."));

        var result = await fixture.Service.SendPhoneLoginCodeAsync(new SendCustomerPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        var request = Assert.Single(fixture.MessageCenterClient.Requests);
        Assert.False(await fixture.Cache.ExistsAsync("customer-auth:phone-verification:code:13800138000", CancellationToken.None));
        Assert.False(await fixture.Cache.ExistsAsync("customer-auth:phone-verification:send-interval:13800138000", CancellationToken.None));

        var loginResult = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = "13800138000",
            VerificationCode = request.VerificationCode
        }, CancellationToken.None);

        Assert.True(loginResult.IsFailure);
        Assert.Equal("Verification code has expired or was not requested.", loginResult.Error.Message);
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldSendRandomCodeUsingPhoneNumberOnly()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.SendPhoneLoginCodeAsync(new SendCustomerPhoneLoginCodeRequest
        {
            PhoneNumber = "13800138000"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var request = Assert.Single(fixture.MessageCenterClient.Requests);
        Assert.Equal("13800138000", request.PhoneNumber);
        Assert.Matches("^[0-9]{6}$", request.VerificationCode);
        Assert.InRange(request.ExpiresIn, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(5));
        Assert.True(await fixture.Cache.ExistsAsync("customer-auth:phone-verification:code:13800138000", CancellationToken.None));
        Assert.True(await fixture.Cache.ExistsAsync("customer-auth:phone-verification:send-interval:13800138000", CancellationToken.None));

        var rawCacheValue = fixture.Cache.GetRawValue("customer-auth:phone-verification:code:13800138000");
        Assert.NotNull(rawCacheValue);
        Assert.DoesNotContain(request.VerificationCode, JsonSerializer.Serialize(rawCacheValue, rawCacheValue.GetType()));
    }

    [Fact]
    public async Task PhoneLoginAsync_ShouldAutoRegisterCustomerAndReturnTokens()
    {
        var fixture = CreateFixture();
        const string phoneNumber = "13800138000";
        var verificationCode = await IssueCodeAsync(fixture, phoneNumber);

        var result = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = phoneNumber,
            VerificationCode = verificationCode
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var customer = Assert.Single(fixture.Repository.Customers);
        Assert.Equal(phoneNumber, customer.PhoneNumber);
        Assert.Equal("用户8000", customer.NickName);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
        Assert.Equal(TokenParties.Customer, ReadTokenParty(result.Value.AccessToken));
        Assert.Equal(1, fixture.UnitOfWork.CommitCount);
    }

    [Fact]
    public async Task PhoneLoginAsync_ShouldReuseExistingCustomer()
    {
        var fixture = CreateFixture();
        const string phoneNumber = "13800138000";
        fixture.Repository.AddCustomer(phoneNumber);
        var verificationCode = await IssueCodeAsync(fixture, phoneNumber);

        var result = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = phoneNumber,
            VerificationCode = verificationCode
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.Repository.Customers);
        Assert.Equal(1, fixture.UnitOfWork.CommitCount);
    }

    [Fact]
    public async Task PhoneLoginAsync_ShouldExpireCodeAfterFiveInvalidAttempts()
    {
        var fixture = CreateFixture();
        const string phoneNumber = "13800138000";
        var verificationCode = await IssueCodeAsync(fixture, phoneNumber);
        var invalidCode = verificationCode == "000000" ? "111111" : "000000";

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            var invalidResult = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
            {
                PhoneNumber = phoneNumber,
                VerificationCode = invalidCode
            }, CancellationToken.None);

            Assert.True(invalidResult.IsFailure);
            Assert.Equal("Verification code is invalid.", invalidResult.Error.Message);
        }

        var exhaustedResult = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = phoneNumber,
            VerificationCode = invalidCode
        }, CancellationToken.None);

        Assert.True(exhaustedResult.IsFailure);
        Assert.Equal("Verification code has expired or was not requested.", exhaustedResult.Error.Message);

        var replayResult = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = phoneNumber,
            VerificationCode = verificationCode
        }, CancellationToken.None);

        Assert.True(replayResult.IsFailure);
        Assert.Equal("Verification code has expired or was not requested.", replayResult.Error.Message);
        Assert.Equal(0, fixture.UnitOfWork.CommitCount);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRotateRefreshToken()
    {
        var fixture = CreateFixture();
        fixture.Repository.AddCustomer("13800138000");
        var verificationCode = await IssueCodeAsync(fixture, "13800138000");

        var loginResult = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = "13800138000",
            VerificationCode = verificationCode
        }, CancellationToken.None);

        var refreshResult = await fixture.Service.RefreshAsync(new CustomerRefreshTokenRequest
        {
            RefreshToken = loginResult.Value!.RefreshToken
        }, CancellationToken.None);

        Assert.True(refreshResult.IsSuccess);
        Assert.NotEqual(loginResult.Value.RefreshToken, refreshResult.Value!.RefreshToken);
        Assert.Equal(TokenParties.Customer, ReadTokenParty(refreshResult.Value.AccessToken));

        var oldRefreshValidation = await fixture.RefreshTokenService.ValidateAsync(loginResult.Value.RefreshToken, CancellationToken.None);
        var newRefreshValidation = await fixture.RefreshTokenService.ValidateAsync(refreshResult.Value.RefreshToken, CancellationToken.None);

        Assert.True(oldRefreshValidation.IsFailure);
        Assert.Equal(ErrorCodes.Unauthorized, oldRefreshValidation.Error.Code);
        Assert.True(newRefreshValidation.IsSuccess);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnUnauthorized_WhenRefreshTokenIsMalformed()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.RefreshAsync(new CustomerRefreshTokenRequest
        {
            RefreshToken = "malformed-token"
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Unauthorized, result.Error.Code);
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnForbidden_WhenCustomerIsDisabled()
    {
        var fixture = CreateFixture();
        var customer = fixture.Repository.AddCustomer("13800138000");
        var verificationCode = await IssueCodeAsync(fixture, "13800138000");

        var loginResult = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = "13800138000",
            VerificationCode = verificationCode
        }, CancellationToken.None);

        fixture.Repository.SetCustomerActive(customer.Id, false);

        var refreshResult = await fixture.Service.RefreshAsync(new CustomerRefreshTokenRequest
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
        var customer = fixture.Repository.AddCustomer("13800138000");
        fixture.CurrentUser.UserId = customer.Id;

        var firstCode = await IssueCodeAsync(fixture, "13800138000");
        var firstLogin = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = "13800138000",
            VerificationCode = firstCode
        }, CancellationToken.None);

        await fixture.Cache.RemoveAsync("customer-auth:phone-verification:send-interval:13800138000", CancellationToken.None);

        var secondCode = await IssueCodeAsync(fixture, "13800138000");
        var secondLogin = await fixture.Service.PhoneLoginAsync(new CustomerPhoneLoginRequest
        {
            PhoneNumber = "13800138000",
            VerificationCode = secondCode
        }, CancellationToken.None);

        var logoutResult = await fixture.Service.LogoutAsync(new CustomerLogoutRequest
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

    private static AuthenticationFixture CreateFixture()
    {
        var cache = new FakeCacheService();
        var repository = new FakeCustomerCommandRepository();
        var unitOfWork = new FakeCustomerAccountUnitOfWork();
        var currentUser = new FakeCurrentUser();
        var messageCenterClient = new FakeMessageCenterClient();
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "Explore.Customer.Tests",
            Audience = "Explore.Customer.Tests.Clients",
            SigningKey = "Explore.Customer.Tests.DevSigningKey.ReplaceBeforeProduction.2026",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7
        });
        var verificationOptions = Options.Create(new PhoneVerificationCodeOptions());
        var phoneVerificationCodeService = new PhoneVerificationCodeService(cache, verificationOptions);
        var jwtTokenService = new JwtTokenService(jwtOptions);
        var refreshTokenService = new CustomerRefreshTokenService(cache, jwtOptions);
        var authenticationService = new CustomerAuthenticationAppService(
            repository,
            currentUser,
            jwtTokenService,
            messageCenterClient,
            phoneVerificationCodeService,
            refreshTokenService,
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
        var issueResult = await fixture.PhoneVerificationCodeService.IssueAsync("customer-auth", phoneNumber, CancellationToken.None);
        Assert.True(issueResult.IsSuccess);
        return issueResult.Value!.Code;
    }

    private static string? ReadTokenParty(string accessToken)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        return token.Claims.SingleOrDefault(x => x.Type == SecurityClaimNames.TokenParty)?.Value;
    }

    private sealed record AuthenticationFixture(
        CustomerAuthenticationAppService Service,
        ICustomerRefreshTokenService RefreshTokenService,
        IPhoneVerificationCodeService PhoneVerificationCodeService,
        FakeCustomerCommandRepository Repository,
        FakeCustomerAccountUnitOfWork UnitOfWork,
        FakeCacheService Cache,
        FakeCurrentUser CurrentUser,
        FakeMessageCenterClient MessageCenterClient);
}

