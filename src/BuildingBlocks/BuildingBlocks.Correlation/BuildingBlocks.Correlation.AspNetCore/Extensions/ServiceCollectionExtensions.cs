using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Correlation.AspNetCore.ContextAccessors;
using BuildingBlocks.Correlation.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Correlation.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrelation(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Scoped<ICorrelationContextAccessor, CorrelationContextAccessor>());
            return services;
        }

        public static IApplicationBuilder UseCorrelation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}

