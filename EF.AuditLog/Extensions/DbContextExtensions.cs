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
        if (!IsModified(entityEntry) || IsEntityExcluded(entityEntry))
        {
            return null;
        }

        var properties = GetPropertiesWithoutIgnored(entityEntry);

        var history = createHistoryFactory();

        history.TableName = entityEntry.GetTableNameOrDefault();
        history.EntityId = entityEntry.GetPrimaryKey();
        history.IsOwned = entityEntry.Metadata.IsOwned();

        switch (entityEntry.State)
        {
            case EntityState.Added:
                AddedState(history, properties);
                break;

            case EntityState.Deleted:
                DeletedState(history, properties);
                break;

            case EntityState.Modified:
                ModifiedState(history, properties);
                break;

            case EntityState.Detached:
            case EntityState.Unchanged:
                return null;

            default:
                throw new NotSupportedException("Not supported entity state.");
        }

        return history;
    }

    private static void AddedState(AuditEntry history, IEnumerable<PropertyEntry> properties)
    {
        var json = new Dictionary<string, object?>();

        foreach (var prop in properties)
        {
            if (prop.Metadata.IsKey())
            {
                continue;
            }

            json[prop.Metadata.Name] = prop.CurrentValue;
        }

        history.ModificationType = EntityState.Added;
        history.OriginalValues = JsonSerializer.Serialize(json, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);
    }

    private static void ModifiedState(
        AuditEntry history,
        IEnumerable<PropertyEntry> properties)
    {
        var originalValues = new Dictionary<string, object?>();
        var updatedValues = new Dictionary<string, object?>();

        foreach (var prop in properties.Where(x => x.IsModified))
        {
            originalValues[prop.Metadata.Name] = prop.OriginalValue;
            updatedValues[prop.Metadata.Name] = prop.CurrentValue;
        }

        history.ModificationType = EntityState.Modified;
        history.OriginalValues = JsonSerializer.Serialize(originalValues, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);
        history.UpdatedValues = JsonSerializer.Serialize(updatedValues, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);
    }

    private static void DeletedState(
        AuditEntry history,
        IEnumerable<PropertyEntry> properties)
    {
        var json = new Dictionary<string, object?>();

        foreach (var prop in properties)
        {
            json[prop.Metadata.Name] = prop.OriginalValue;
        }

        history.ModificationType = EntityState.Deleted;
        history.OriginalValues = JsonSerializer.Serialize(json, EntityAuditConfigurationsRegistry.AuditSettings.JsonSerializerOptions);
    }

    private static PropertyEntry[] GetPropertiesWithoutIgnored(EntityEntry entry)
    {
        var ignoredProperties = EntityAuditConfigurationsRegistry.GetIgnoredProperties(entry);

        return entry.Properties
            .Where(f => !ignoredProperties.Contains(f.Metadata.Name))
            .ToArray();
    }

    private static bool IsModified(this EntityEntry entityEntry) => ModifiedStates.Contains(entityEntry.State);

    private static bool IsEntityExcluded(this EntityEntry entry)
    {
        return EntityAuditConfigurationsRegistry.IsEntityExcluded(entry);
    }
}