using BuildingBlocks.Security.Hashing.Abstractions;
using BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security.Hashing.AspNetCoreIdentity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAspNetCoreIdentityPasswordHashService(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        return services;
    }
}

