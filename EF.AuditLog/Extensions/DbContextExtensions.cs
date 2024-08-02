using System.Text.Json;

using EF.AuditLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EF.AuditLog.Extensions;

/// <summary>
/// Extension methods for <see cref="DbContext"/>.
/// </summary>
public static class DbContextExtensions
{
    private static readonly IEnumerable<EntityState> ModifiedStates = new[]
    {
        EntityState.Added,
        EntityState.Modified,
        EntityState.Deleted,
    };

    /// <summary>
    /// Ensures the automatic history.
    /// </summary>
    /// <param name="context">The context.</param>
    public static void AuditEntries(this DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => ModifiedStates.Contains(e.State))
            .ToArray();

        foreach (var entry in entries)
        {
            context.AuditEntry(entry);
        }
    }

    /// <summary>
    /// Ensures the automatic history.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="createHistoryFactory">The create history factory.</param>
    /// <typeparam name="TAuditEntry">The type of the audit entry.</typeparam>
    public static void AuditEntries<TAuditEntry>(
        this DbContext context,
        Func<TAuditEntry> createHistoryFactory)
        where TAuditEntry : AuditEntry
    {
        var entries = context.ChangeTracker.Entries()
            .Where(x => ModifiedStates.Contains(x.State))
            .ToArray();

        foreach (var entry in entries)
        {
            context.AuditEntry(entry, createHistoryFactory);
        }
    }

    /// <summary>
    /// Ensures the automatic history.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="entityEntry">The entity entry.</param>
    private static void AuditEntry(this DbContext context, EntityEntry entityEntry)
    {
        var auditEntry = entityEntry.AuditEntry(() => new AuditEntry());

        if (auditEntry == null)
        {
            return;
        }

        context.Set<AuditEntry>().Add(auditEntry);
    }

    /// <summary>
    /// Ensures the automatic history.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="entityEntry">The entity entry.</param>
    /// <param name="createHistoryFactory">The create history factory.</param>
    /// <typeparam name="TAuditEntry">The type of the audit entry.</typeparam>
    private static void AuditEntry<TAuditEntry>(
        this DbContext context,
        EntityEntry entityEntry,
        Func<TAuditEntry> createHistoryFactory)
        where TAuditEntry : AuditEntry
    {
        var auditEntry = entityEntry.AuditEntry(createHistoryFactory);

        if (auditEntry == null)
        {
            return;
        }

        context.Set<TAuditEntry>().Add(auditEntry);
    }

    private static TAuditEntry? AuditEntry<TAuditEntry>(
        this EntityEntry entityEntry,
        Func<TAuditEntry> createHistoryFactory)
        where TAuditEntry : AuditEntry
    {
        if (!IsModified(entityEntry) ||
	        IsEntityExcluded(entityEntry) ||
	        entityEntry.Metadata.IsOwned())
        {
            return null;
        }

        var history = createHistoryFactory();

        history.TableName = entityEntry.GetTableNameOrDefault();
        history.EntityId = entityEntry.GetPrimaryKey();
        history.IsOwned = entityEntry.Metadata.IsOwned();

        switch (entityEntry.State)
        {
            case EntityState.Added:
                AddedState(history, entityEntry);
                break;

            case EntityState.Deleted:
                DeletedState(history, entityEntry);
                break;

            case EntityState.Modified:
                ModifiedState(history, entityEntry);
                break;

            case EntityState.Detached:
            case EntityState.Unchanged:
                return null;

            default:
                throw new NotSupportedException("Not supported entity state.");
        }

        return history;
    }

    private static void AddedState(
	    AuditEntry history,
	    EntityEntry entityEntry)
    {
        var json = new Dictionary<string, object?>();

        var properties = GetPropertiesWithoutIgnored(entityEntry);
        foreach (var prop in properties)
        {
            if (prop.Metadata.IsKey())
            {
                continue;
            }

            json[prop.Metadata.Name] = prop.CurrentValue;
        }

        var ownedEntities = GetOwnedEntities(entityEntry);
        foreach (var (key, value) in ownedEntities)
        {
	        json[key] = value;
        }

        history.ModificationType = EntityState.Added;
        history.OriginalValues = JsonSerializer.Serialize(json, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);
    }

    private static void DeletedState(
	    AuditEntry history,
	    EntityEntry entityEntry)
    {
	    var json = new Dictionary<string, object?>();

	    var properties = GetPropertiesWithoutIgnored(entityEntry);
	    foreach (var prop in properties)
	    {
		    if (prop.Metadata.IsKey())
		    {
			    continue;
		    }

		    json[prop.Metadata.Name] = prop.OriginalValue;
	    }

	    var ownedEntities = GetOwnedEntities(entityEntry);
	    foreach (var (key, value) in ownedEntities)
	    {
		    json[key] = value;
	    }

	    history.ModificationType = EntityState.Deleted;
	    history.OriginalValues = JsonSerializer.Serialize(json, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);
    }

    private static void ModifiedState(
        AuditEntry history,
        EntityEntry entityEntry)
    {
        var originalValues = new Dictionary<string, object?>();
        var updatedValues = new Dictionary<string, object?>();

        var properties = GetPropertiesWithoutIgnored(entityEntry);
        foreach (var prop in properties.Where(x => x.IsModified))
        {
            originalValues[prop.Metadata.Name] = prop.OriginalValue;
            updatedValues[prop.Metadata.Name] = prop.CurrentValue;
        }

        var ownedEntities = GetModifiedOwnedEntities(entityEntry);
        for (int i = 0; i < ownedEntities.OriginalValues.Count; i++)
        {
	        var originalEntity = ownedEntities.OriginalValues.ElementAt(i);
	        var updatedEntity = ownedEntities.CurrentValues.ElementAt(i);

	        originalValues[originalEntity.Key] = originalEntity.Value;
	        updatedValues[updatedEntity.Key] = updatedEntity.Value;
        }

        history.ModificationType = EntityState.Modified;
        history.OriginalValues = JsonSerializer.Serialize(originalValues, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);
        history.UpdatedValues = JsonSerializer.Serialize(updatedValues, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);

        return;

        static (IDictionary<string, object?> OriginalValues, IDictionary<string, object?> CurrentValues) GetModifiedOwnedEntities(EntityEntry entityEntry)
        {
	        var originalValues = new Dictionary<string, object?>();
	        var currentValues = new Dictionary<string, object?>();

	        var references = entityEntry.References
		        .Where(x => x.TargetEntry != null)
		        .Where(x => x.TargetEntry!.Metadata.IsOwned())
		        .Where(x => x.TargetEntry!.IsModified());

	        foreach (var reference in references)
	        {
		        var modifiedProperties = reference.TargetEntry!.Properties
			        .Where(x => x.IsModified)
			        .ToDictionary(x => x.Metadata.Name, x => x.CurrentValue);

		        var originalProperties = reference.TargetEntry!.Properties
			        .Where(x => x.IsModified)
			        .ToDictionary(x => x.Metadata.Name, x => x.OriginalValue);

		        // Рекурсивно получаем вложенные owned entities
		        var (nestedOriginalEntities, nestedCurrentEntities) = GetModifiedOwnedEntities(reference.TargetEntry);

		        // Объединяем измененные свойства и вложенные сущности в текущие значения
		        foreach (var nestedEntity in nestedCurrentEntities)
		        {
			        modifiedProperties[nestedEntity.Key] = nestedEntity.Value;
		        }

		        // Объединяем измененные свойства и вложенные сущности в оригинальные значения
		        foreach (var nestedEntity in nestedOriginalEntities)
		        {
			        originalProperties[nestedEntity.Key] = nestedEntity.Value;
		        }

		        currentValues[reference.Metadata.Name] = modifiedProperties;
		        originalValues[reference.Metadata.Name] = originalProperties;
	        }

	        return (originalValues, currentValues);
        }
    }

    private static PropertyEntry[] GetPropertiesWithoutIgnored(EntityEntry entry)
    {
        var ignoredProperties = EntityAuditConfigurationsRegistry.GetIgnoredProperties(entry);

        return entry.Properties
            .Where(f => !ignoredProperties.Contains(f.Metadata.Name))
            .ToArray();
    }

    private static bool IsModified(this EntityEntry entityEntry) => ModifiedStates.Contains(entityEntry.State);

    private static bool IsEntityExcluded(this EntityEntry entry) => EntityAuditConfigurationsRegistry.IsEntityExcluded(entry);

    private static IDictionary<string, object?> GetOwnedEntities(EntityEntry entityEntry)
    {
	    var ownedEntities = new Dictionary<string, object?>();

	    var references = entityEntry.References
		    .Where(x => x.TargetEntry != null)
		    .Where(x => x.TargetEntry!.Metadata.IsOwned())
		    .Where(x => x.TargetEntry!.IsModified());

	    foreach (var reference in references)
	    {
		    ownedEntities[reference.Metadata.Name] = reference.CurrentValue;
	    }

	    return ownedEntities;
    }
}