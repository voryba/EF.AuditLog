using System.Reflection;
using EF.AuditLog.Abstractions;

namespace EF.AuditLog;

/// <summary>
/// Represents a registry for audit configurations.
/// </summary>
internal static class ConfigurationRegistry
{
    private static readonly Dictionary<Type, object> Configurations = new();

    /// <summary>
    /// Gets the audit configurations.
    /// </summary>
    /// <returns>The audit configurations.</returns>
    internal static Dictionary<Type, object> GetConfigurations()
    {
        return Configurations;
    }

    /// <summary>
    /// Gets the audit configuration for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The audit configuration for the specified entity type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no configuration is registered for the specified entity type.</exception>
    internal static IAuditConfiguration<TEntity>? GetConfiguration<TEntity>()
        where TEntity : class
    {
        if (Configurations.TryGetValue(typeof(TEntity), out var configuration))
        {
            return (IAuditConfiguration<TEntity>)configuration;
        }

        // throw new InvalidOperationException($"No configuration registered for {typeof(TEntity).Name}");
        return null;
    }

    /// <summary>
    /// Registers the specified audit configuration.
    /// </summary>
    /// <param name="configuration">The audit configuration to register.</param>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    internal static void RegisterConfiguration<TEntity>(IAuditConfiguration<TEntity> configuration)
        where TEntity : class
    {
        Configurations[typeof(TEntity)] = configuration;
    }

    /// <summary>
    /// Registers audit configurations from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to register audit configurations from.</param>
    internal static void RegisterConfigurationsFromAssembly(Assembly assembly)
    {
        var configurationTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(t => t.Interface.IsGenericType && t.Interface.GetGenericTypeDefinition() == typeof(IAuditConfiguration<>))
            .ToArray();

        foreach (var configType in configurationTypes)
        {
            var entityType = configType.Interface.GetGenericArguments()[0];
            var configurationInstance = Activator.CreateInstance(configType.Type);
            Configurations[entityType] = configurationInstance!;
        }
    }
}