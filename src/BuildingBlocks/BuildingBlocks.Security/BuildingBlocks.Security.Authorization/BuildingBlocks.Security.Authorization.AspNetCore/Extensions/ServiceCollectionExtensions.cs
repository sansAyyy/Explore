using BuildingBlocks.Security.Authentication.Constants;
using BuildingBlocks.Security.Authorization.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security.Authorization.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTokenPartyAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.AdminOnly,
                policy => ConfigureTokenPartyPolicy(policy, TokenParties.Admin));
            options.AddPolicy(
                AuthorizationPolicies.CustomerOnly,
                policy => ConfigureTokenPartyPolicy(policy, TokenParties.Customer));
        });

        return services;
    }

    private static void ConfigureTokenPartyPolicy(AuthorizationPolicyBuilder policy, string tokenParty)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(SecurityClaimNames.TokenParty, tokenParty);
    }
}

