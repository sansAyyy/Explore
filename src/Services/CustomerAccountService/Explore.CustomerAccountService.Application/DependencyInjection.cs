using BuildingBlocks.DependencyInjection.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.CustomerAccountService.Application;

public class DependencyInjection : IModuleDependencyRegistrar
{
    public IServiceCollection ModuleDependencyInjection(IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}

