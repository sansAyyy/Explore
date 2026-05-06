using BuildingBlocks.OpenApi.AspNetCore.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace BuildingBlocks.OpenApi.AspNetCore.Extensions;

public static class ServiceOpenApiExtensions
{
    public static IServiceCollection AddServiceOpenApi(
        this IServiceCollection services,
        Action<ServiceOpenApiOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var serviceOpenApiOptions = new ServiceOpenApiOptions();
        configure(serviceOpenApiOptions);

        ArgumentException.ThrowIfNullOrWhiteSpace(serviceOpenApiOptions.Title);

        if (serviceOpenApiOptions.UseBearerSecurity)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(serviceOpenApiOptions.BearerSchemeName);
        }

        services.AddSingleton(serviceOpenApiOptions);

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = serviceOpenApiOptions.Title,
                    Version = serviceOpenApiOptions.Version,
                    Description = serviceOpenApiOptions.Description
                };

                if (!serviceOpenApiOptions.UseBearerSecurity)
                {
                    return Task.CompletedTask;
                }

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
                document.Components.SecuritySchemes[serviceOpenApiOptions.BearerSchemeName] =
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Description = serviceOpenApiOptions.BearerDescription
                    };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                if (!serviceOpenApiOptions.UseBearerSecurity ||
                    !RequiresAuthorization(context.Description))
                {
                    return Task.CompletedTask;
                }

                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(
                        serviceOpenApiOptions.BearerSchemeName,
                        context.Document,
                        null)] = new List<string>()
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication MapServiceOpenApi(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        var serviceOpenApiOptions = app.Services.GetRequiredService<ServiceOpenApiOptions>();

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle(serviceOpenApiOptions.Title);

            if (!serviceOpenApiOptions.UseBearerSecurity)
            {
                return;
            }

            options.AddPreferredSecuritySchemes(new[] { serviceOpenApiOptions.BearerSchemeName });

            if (serviceOpenApiOptions.EnablePersistentAuthentication)
            {
                options.EnablePersistentAuthentication();
            }
        });

        return app;
    }

    private static bool RequiresAuthorization(ApiDescription description)
    {
        if (string.IsNullOrWhiteSpace(description.RelativePath) ||
            !description.RelativePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var endpointMetadata = description.ActionDescriptor.EndpointMetadata;
        return endpointMetadata.OfType<IAllowAnonymous>().Any() is false;
    }
}

