using BuildingBlocks.OpenApi.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class OpenApiAspNetCoreTests
{
    [Fact]
    public void AddServiceOpenApi_ShouldRequireTitle()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddServiceOpenApi(options => options.Title = ""));

        Assert.Equal("serviceOpenApiOptions.Title", exception.ParamName);
    }

    [Fact]
    public void AddServiceOpenApi_ShouldRequireBearerSchemeName_WhenBearerSecurityIsEnabled()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddServiceOpenApi(options =>
            {
                options.Title = "Test API";
                options.UseBearerSecurity = true;
                options.BearerSchemeName = "";
            }));

        Assert.Equal("serviceOpenApiOptions.BearerSchemeName", exception.ParamName);
    }

    [Fact]
    public void MapServiceOpenApi_ShouldNotMapEndpoints_WhenEnvironmentIsNotDevelopment()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(OpenApiAspNetCoreTests).Assembly.FullName,
            EnvironmentName = Environments.Production
        });

        builder.Services.AddServiceOpenApi(options => options.Title = "Test API");

        var app = builder.Build();
        var endpointDataSources = app.Services.GetServices<EndpointDataSource>().ToList();
        var endpointsBefore = endpointDataSources.Sum(source => source.Endpoints.Count);

        var returned = app.MapServiceOpenApi();

        var endpointsAfter = endpointDataSources.Sum(source => source.Endpoints.Count);

        Assert.Same(app, returned);
        Assert.Equal(endpointsBefore, endpointsAfter);
    }
}

