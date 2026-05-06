using BuildingBlocks.CurrentUser.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CurrentUser.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAspNetCoreCurrentUser(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUser, BuildingBlocks.CurrentUser.AspNetCore.Services.CurrentUser>();
        return services;
    }
}

