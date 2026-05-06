using System.Reflection;
using BuildingBlocks.DependencyInjection.Abstractions;
using BuildingBlocks.DependencyInjection.Conventional.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class DependencyInjectionConventionalTests
{
    [Fact]
    public void AddConventionalServicesFromAssembly_ShouldRegisterScopedTransientSingletonAndSelfBindings()
    {
        var services = new ServiceCollection();

        services.AddConventionalServicesFromAssembly(typeof(DependencyInjectionConventionalTests).Assembly);

        using var provider = services.BuildServiceProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var scopedOne = firstScope.ServiceProvider.GetRequiredService<ITestScopedService>();
        var scopedTwo = firstScope.ServiceProvider.GetRequiredService<ITestScopedService>();
        var scopedFromOtherScope = secondScope.ServiceProvider.GetRequiredService<ITestScopedService>();
        var transientOne = firstScope.ServiceProvider.GetRequiredService<ITestTransientService>();
        var transientTwo = firstScope.ServiceProvider.GetRequiredService<ITestTransientService>();
        var singletonOne = firstScope.ServiceProvider.GetRequiredService<ITestSingletonService>();
        var singletonTwo = secondScope.ServiceProvider.GetRequiredService<ITestSingletonService>();
        var selfBound = firstScope.ServiceProvider.GetRequiredService<SelfBoundScopedService>();

        Assert.Same(scopedOne, scopedTwo);
        Assert.NotSame(scopedOne, scopedFromOtherScope);
        Assert.NotSame(transientOne, transientTwo);
        Assert.Same(singletonOne, singletonTwo);
        Assert.NotNull(selfBound);
    }

    [Fact]
    public void AddConventionalServicesFromAssembly_ShouldIgnoreMarkerInterfacesAsServiceContracts()
    {
        var services = new ServiceCollection();

        services.AddConventionalServicesFromAssembly(typeof(DependencyInjectionConventionalTests).Assembly);

        using var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IScopeDependency>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ITransientDependency>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ISingletonDependency>());
    }

    [Fact]
    public void AddModules_ShouldDiscoverRegistrar_AndExecuteModuleRegistration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddModules(configuration);

        using var provider = services.BuildServiceProvider();

        var moduleService = provider.GetRequiredService<IModuleRegisteredService>();
        var scopedService = provider.GetRequiredService<ITestScopedService>();

        Assert.Equal("module-registered", moduleService.Name);
        Assert.NotNull(scopedService);
    }

    [Fact]
    public void ApplicationStyleModule_ShouldStillBeAbleToRegisterServicesThroughInterfaceContract()
    {
        var services = new ServiceCollection();
        var module = new FakeModuleDependencyRegistrar();

        module.ModuleDependencyInjection(services, new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IModuleRegisteredService>();

        Assert.Equal("module-registered", service.Name);
    }

    public interface ITestScopedService;

    public interface ITestTransientService;

    public interface ITestSingletonService;

    public interface IModuleRegisteredService
    {
        string Name { get; }
    }

    public sealed class ScopedService : ITestScopedService, IScopeDependency;

    public sealed class TransientService : ITestTransientService, ITransientDependency;

    public sealed class SingletonService : ITestSingletonService, ISingletonDependency;

    public sealed class SelfBoundScopedService : IScopeDependency;

    public sealed class ModuleRegisteredService : IModuleRegisteredService
    {
        public string Name => "module-registered";
    }

    public sealed class FakeModuleDependencyRegistrar : IModuleDependencyRegistrar
    {
        public IServiceCollection ModuleDependencyInjection(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IModuleRegisteredService, ModuleRegisteredService>();
            return services;
        }
    }
}
