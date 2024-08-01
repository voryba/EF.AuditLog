using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EF.AuditLog.Extensions;

/// <summary>
/// Extension methods for <see cref="EntityEntry"/>.
/// </summary>
internal static class EntityEntryExtensions
{
    /// <summary>
    /// Gets the primary key value of the specified entity entry.
    /// </summary>
    /// <param name="entry">The entity entry.</param>
    /// <returns>The primary key value.</returns>
    internal static string GetPrimaryKey(this EntityEntry entry)
    {
        var keyName = entry.Metadata
            .FindPrimaryKey()?.Properties
            .Select(x => x.Name).SingleOrDefault();

        if (keyName == null)
        {
            return string.Empty;
        }

        return entry.Property(keyName).CurrentValue?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the table name of the specified entity entry.
    /// </summary>
    /// <param name="entityEntry">The entity entry.</param>
    /// <returns>The table name.</returns>
    internal static string GetTableNameOrDefault(this EntityEntry entityEntry)
    {
        return entityEntry.Metadata.GetTableName() ?? entityEntry.Metadata.Name;
    }
}