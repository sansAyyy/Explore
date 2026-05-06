using System.Text;
using BuildingBlocks.Security.Authentication.Abstractions;
using BuildingBlocks.Security.Authentication.Jwt.Services;
using BuildingBlocks.Security.Authentication.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Security.Authentication.Jwt.Extensions;

public static class JwtBearerExtensions
{
    public static IServiceCollection AddJwtTokenService(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = JwtOptions.SectionName)
    {
        services.Configure<JwtOptions>(configuration.GetSection(sectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                $"JWT configuration section '{JwtOptions.SectionName}' is required.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };
            });

        return services;
    }
}

