using System.Reflection;
using System.Runtime.Loader;
using BuildingBlocks.DependencyInjection.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace BuildingBlocks.DependencyInjection.Conventional.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Type[] IgnoreServiceInterfaces =
    [
        typeof(IScopeDependency),
        typeof(ITransientDependency),
        typeof(ISingletonDependency),
        typeof(IModuleDependencyRegistrar),
        typeof(IDisposable),
        typeof(IAsyncDisposable)
    ];

    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        var assemblies = GetApplicationAssemblies().ToList();

        foreach (var assembly in assemblies)
        {
            services.AddConventionalServicesFromAssembly(assembly);
        }

        foreach (var registrar in CreateModuleRegistrars(assemblies))
        {
            registrar.ModuleDependencyInjection(services, configuration);
        }

        return services;
    }

    public static IServiceCollection AddConventionalServicesFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var types = GetLoadableTypes(assembly).ToArray();

        RegisterTypes(services, types, typeof(IScopeDependency), services.AddScoped);
        RegisterTypes(services, types, typeof(ITransientDependency), services.AddTransient);
        RegisterTypes(services, types, typeof(ISingletonDependency), services.AddSingleton);

        return services;
    }

    private static IEnumerable<Assembly> GetApplicationAssemblies()
    {
        var assemblies = new List<Assembly>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddAssembly(Assembly assembly)
        {
            var key = assembly.FullName ?? assembly.GetName().Name ?? assembly.Location;
            if (seen.Add(key))
            {
                assemblies.Add(assembly);
            }
        }

        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            AddAssembly(entryAssembly);
        }

        var compileLibraries = DependencyContext.Default?.CompileLibraries
            .Where(library => !library.Serviceable && library.Type != "package")
            .ToList()
            ?? [];

        foreach (var library in compileLibraries)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(library.Name));
            AddAssembly(assembly);
        }

        return assemblies;
    }

    private static IEnumerable<IModuleDependencyRegistrar> CreateModuleRegistrars(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(GetLoadableTypes)
            .Where(type => type.IsClass && !type.IsAbstract && typeof(IModuleDependencyRegistrar).IsAssignableFrom(type))
            .Select(type => Activator.CreateInstance(type))
            .OfType<IModuleDependencyRegistrar>();
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    private static void RegisterTypes(
        IServiceCollection services,
        IEnumerable<Type> types,
        Type markerType,
        Func<Type, Type, IServiceCollection> register)
    {
        var implementations = types
            .Where(type => type.IsClass && !type.IsAbstract && markerType.IsAssignableFrom(type));

        foreach (var implementation in implementations)
        {
            var serviceTypes = implementation.GetInterfaces()
                .Where(serviceType => !IgnoreServiceInterfaces.Contains(serviceType))
                .ToArray();

            if (serviceTypes.Length == 0)
            {
                register(implementation, implementation);
                continue;
            }

            foreach (var serviceType in serviceTypes)
            {
                register(serviceType, implementation);
            }
        }
    }
}
