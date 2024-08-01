using Microsoft.EntityFrameworkCore;

namespace EF.AuditLog.Models;

/// <summary>
/// Represents an audit entry.
/// </summary>
public class AuditEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit entry.
    /// </summary>
    public Guid Id { get; internal set; }

    /// <summary>
    /// Gets or sets the name of the table where the change occurred.
    /// </summary>
    public string TableName { get; internal set; }

    /// <summary>
    /// Gets or sets the identifier of the row that was changed.
    /// </summary>
    public string EntityId { get; internal set; }

    /// <summary>
    /// Gets or sets the changed values in JSON format.
    /// </summary>
    public string OriginalValues { get; internal set; }

    /// <summary>
    /// Gets or sets the changed values in JSON format.
    /// </summary>
    public string? UpdatedValues { get; internal set; }

    /// <summary>
    /// Gets or sets the type of change (e.g., Added, Modified, Deleted).
    /// </summary>
    public EntityState ModificationType { get; internal set; }

    /// <summary>
    /// Gets or sets the date and time when the change was created.
    /// </summary>
    public DateTime Timestamp { get; internal init; } = DateTime.UtcNow;

    /// <summary>
    /// True if the entity is owned by another entity.
    /// </summary>
    public bool IsOwned { get; internal set; }
}