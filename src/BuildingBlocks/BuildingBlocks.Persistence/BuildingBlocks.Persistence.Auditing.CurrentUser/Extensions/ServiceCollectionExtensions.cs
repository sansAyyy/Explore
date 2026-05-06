using BuildingBlocks.Persistence.Auditing.Abstractions;
using BuildingBlocks.Persistence.Auditing.CurrentUser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Persistence.Auditing.CurrentUser.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentUserAuditActorAccessor(this IServiceCollection services)
    {
        services.AddScoped<IAuditActorAccessor, CurrentUserAuditActorAccessor>();
        return services;
    }
}
