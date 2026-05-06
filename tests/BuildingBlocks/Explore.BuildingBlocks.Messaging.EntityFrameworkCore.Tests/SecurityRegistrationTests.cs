using BuildingBlocks.Caching.Abstractions;
using BuildingBlocks.Security.Authentication.Abstractions;
using BuildingBlocks.Security.Authentication.Constants;
using BuildingBlocks.Security.Authentication.Jwt.Extensions;
using BuildingBlocks.Security.Authentication.Jwt.Services;
using BuildingBlocks.Security.Authorization.AspNetCore.Extensions;
using BuildingBlocks.Security.Authorization.Constants;
using BuildingBlocks.Security.Hashing.Abstractions;
using BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Extensions;
using BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Services;
using BuildingBlocks.Security.PhoneVerification.Abstractions;
using BuildingBlocks.Security.PhoneVerification.Extensions;
using BuildingBlocks.Security.PhoneVerification.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class SecurityRegistrationTests
{
    [Fact]
    public void AddJwtTokenService_ShouldRegisterJwtTokenService()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddJwtTokenService(configuration);

        using var provider = services.BuildServiceProvider();
        var jwtTokenService = provider.GetRequiredService<IJwtTokenService>();

        Assert.IsType<JwtTokenService>(jwtTokenService);
    }

    [Fact]
    public void AddPhoneVerificationCode_ShouldRegisterPhoneVerificationCodeService()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddSingleton<ICacheService, NoOpCacheService>();
        services.AddPhoneVerificationCode(configuration);

        using var provider = services.BuildServiceProvider();
        var phoneVerificationCodeService = provider.GetRequiredService<IPhoneVerificationCodeService>();

        Assert.IsType<PhoneVerificationCodeService>(phoneVerificationCodeService);
    }

    [Fact]
    public void AddAspNetCoreIdentityPasswordHashService_ShouldRegisterPasswordHashService()
    {
        var services = new ServiceCollection();

        services.AddAspNetCoreIdentityPasswordHashService();

        using var provider = services.BuildServiceProvider();
        var passwordHashService = provider.GetRequiredService<IPasswordHashService>();

        Assert.IsType<PasswordHashService>(passwordHashService);
    }

    [Fact]
    public async Task AddTokenPartyAuthorizationPolicies_ShouldRegisterAdminAndCustomerPolicies()
    {
        var services = new ServiceCollection();

        services.AddTokenPartyAuthorizationPolicies();

        using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var adminPolicy = await policyProvider.GetPolicyAsync(AuthorizationPolicies.AdminOnly);
        var customerPolicy = await policyProvider.GetPolicyAsync(AuthorizationPolicies.CustomerOnly);

        AssertPolicyRequiresTokenParty(adminPolicy, TokenParties.Admin);
        AssertPolicyRequiresTokenParty(customerPolicy, TokenParties.Customer);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtOptions:Issuer"] = "Explore",
                ["JwtOptions:Audience"] = "Explore.Clients",
                ["JwtOptions:SigningKey"] = "change-this-local-jwt-signing-key",
                ["PhoneVerificationCodeOptions:Length"] = "6",
                ["PhoneVerificationCodeOptions:ExpirationSeconds"] = "300",
                ["PhoneVerificationCodeOptions:ResendCooldownSeconds"] = "60",
                ["PhoneVerificationCodeOptions:MaxVerifyAttempts"] = "5"
            })
            .Build();
    }

    private static void AssertPolicyRequiresTokenParty(AuthorizationPolicy? policy, string expectedTokenParty)
    {
        Assert.NotNull(policy);

        var claimRequirement = policy!.Requirements
            .OfType<ClaimsAuthorizationRequirement>()
            .SingleOrDefault(requirement => requirement.ClaimType == SecurityClaimNames.TokenParty);

        Assert.NotNull(claimRequirement);
        Assert.Contains(expectedTokenParty, claimRequirement!.AllowedValues ?? []);
    }

    private sealed class NoOpCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            return Task.FromResult(false);
        }
    }
}

