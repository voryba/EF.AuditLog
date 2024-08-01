using System.Collections.Concurrent;
using System.Reflection;
using EF.AuditLog.Abstractions;
using EF.AuditLog.Options;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EF.AuditLog;

/// <summary>
/// Entity audit settings.
/// </summary>
internal static class EntityAuditConfigurationsRegistry
{
    /// <summary>
    /// Gets the ignored properties for the specified entity entry.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, EntityAuditConfiguration> EntityAuditOptions = new();

    /// <summary>
    /// Gets the audit settings.
    /// </summary>
    public static AuditSettings AuditSettings { get; private set; } = new();

    /// <summary>
    /// Gets the ignored properties for the specified entity entry.
    /// </summary>
    /// <param name="entityEntry">The entity entry.</param>
    /// <returns>The ignored properties.</returns>
    public static IReadOnlyList<string> GetIgnoredProperties(EntityEntry entityEntry)
    {
	    var options = GetOrAddConfiguration(entityEntry);

	    return options.IgnoredProperties;
    }

    /// <summary>
    /// Determines whether the specified entity entry is excluded.
    /// </summary>
    /// <param name="entityEntry">The entity entry.</param>
    /// <returns><c>true</c> if the entity entry is excluded; otherwise, <c>false</c>.</returns>
    public static bool IsEntityExcluded(EntityEntry entityEntry)
    {
	    var options = GetOrAddConfiguration(entityEntry);

	    return options.ExcludeEntity;
    }

    private static EntityAuditConfiguration GetOrAddConfiguration(EntityEntry entityEntry)
    {
	    return EntityAuditOptions.GetOrAdd(entityEntry.Entity.GetType(), _ =>
	    {
		    var configuration = LoadEntityConfiguration(entityEntry);
		    return configuration ?? new EntityAuditConfiguration(Array.Empty<string>(), ExcludeEntity: AuditSettings.OnlyConfiguredAudited);
	    });
    }

    /// <summary>
    /// Sets the audit settings.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public static void SetAuditSettings(AuditSettings settings)
    {
        AuditSettings = settings;
    }

    private static EntityAuditConfiguration? LoadEntityConfiguration(EntityEntry entityEntry)
    {
        var entity = entityEntry.Entity;
        var entityType = entity.GetType();

        var configurations = ConfigurationRegistry.GetConfigurations();

        if (!configurations.TryGetValue(entityType, out var configuration))
        {
	        return null;
        }

        var configType = configuration.GetType();

        var configInstance = Activator.CreateInstance(configType);
        var configureMethod = configType.GetMethod(nameof(IAuditConfiguration<object>.Configure));

        var builderType = typeof(AuditTypeConfigurationBuilder<>).MakeGenericType(entityType);
        var builderInstance = Activator.CreateInstance(builderType);

        configureMethod!.Invoke(configInstance, new[] { builderInstance });

        return (EntityAuditConfiguration)builderInstance!
            .GetType()
            .GetMethod(nameof(AuditTypeConfigurationBuilder<object>.Build), BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(builderInstance, null)!;
    }
}