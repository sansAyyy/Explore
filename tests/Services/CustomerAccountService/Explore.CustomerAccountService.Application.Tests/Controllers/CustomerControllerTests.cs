using Explore.CustomerAccountService.Api.Controllers;
using BuildingBlocks.Security.Authorization.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Explore.CustomerAccountService.Application.Tests.Controllers;

public sealed class CustomerControllerTests
{
    [Fact]
    public void RefreshAsync_ShouldAllowAnonymous()
    {
        var method = typeof(CustomerAuthenticationController).GetMethod(nameof(CustomerAuthenticationController.RefreshAsync));

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
        Assert.Null(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public void LogoutAsync_ShouldRequireAuthorization()
    {
        var method = typeof(CustomerAuthenticationController).GetMethod(nameof(CustomerAuthenticationController.LogoutAsync));
        var attribute = method?.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(method);
        Assert.NotNull(attribute);
        Assert.Equal(AuthorizationPolicies.CustomerOnly, attribute!.Policy);
        Assert.Null(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public void CustomerCurrentUserController_ShouldRequireAuthorization()
    {
        var attribute = typeof(CustomerCurrentUserController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(AuthorizationPolicies.CustomerOnly, attribute!.Policy);
    }

    [Fact]
    public void AdminCustomersController_ShouldRequireAdminAuthorization()
    {
        var attribute = typeof(AdminCustomersController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute!.Policy);
    }

    [Fact]
    public void ActivateAsync_ShouldNotAllowAnonymous()
    {
        var method = typeof(AdminCustomersController).GetMethod(nameof(AdminCustomersController.ActivateAsync));
        var authorizeAttribute = method?.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(method);
        Assert.Null(method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
        Assert.Null(authorizeAttribute);
    }

    [Fact]
    public void DeactivateAsync_ShouldNotAllowAnonymous()
    {
        var method = typeof(AdminCustomersController).GetMethod(nameof(AdminCustomersController.DeactivateAsync));
        var authorizeAttribute = method?.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(method);
        Assert.Null(method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
        Assert.Null(authorizeAttribute);
    }

    [Fact]
    public void GetNotificationProfileAsync_ShouldAllowAnonymous()
    {
        var method = typeof(UsersController).GetMethod(nameof(UsersController.GetNotificationProfileAsync));

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
    }
}

