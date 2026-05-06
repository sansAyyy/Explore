using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.DependencyInjection.Abstractions;

public interface IModuleDependencyRegistrar
{
    IServiceCollection ModuleDependencyInjection(IServiceCollection services, IConfiguration configuration);
}
