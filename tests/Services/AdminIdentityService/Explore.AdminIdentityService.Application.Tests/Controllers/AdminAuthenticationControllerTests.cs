using Explore.AdminIdentityService.Api.Controllers;
using BuildingBlocks.Security.Authorization.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Explore.AdminIdentityService.Application.Tests.Controllers;

public sealed class AdminAuthenticationControllerTests
{
    [Fact]
    public void RefreshAsync_ShouldAllowAnonymous()
    {
        var method = typeof(AdminAuthenticationController).GetMethod(nameof(AdminAuthenticationController.RefreshAsync));

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
        Assert.Null(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public void LogoutAsync_ShouldRequireAuthorization()
    {
        var method = typeof(AdminAuthenticationController).GetMethod(nameof(AdminAuthenticationController.LogoutAsync));
        var attribute = method?.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(method);
        Assert.NotNull(attribute);
        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute!.Policy);
        Assert.Null(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
    }
}

