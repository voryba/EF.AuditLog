using System.Text.Json;

namespace EF.AuditLog.Options;

/// <summary>
/// Represents the settings for the audit trail.
/// </summary>
internal sealed class AuditSettings
{
    /// <summary>
    /// Gets the default <see cref="AuditSettings"/>.
    /// </summary>
    public static AuditSettings Default { get; } = new();

    /// <summary>
    /// The Json Serializer Options.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether only configured audited entities should be audited.
    /// </summary>
    public bool OnlyConfiguredAudited { get; set; } = false;
}