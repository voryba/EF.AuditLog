using System.Text.Json;

namespace EF.AuditLog.Options;

/// <summary>
/// Audit settings builder.
/// </summary>
public class AuditSettingsBuilder
{
    private readonly AuditSettings _settings = new();

    /// <summary>
    /// Builds the json serializer settings.
    /// </summary>
    /// <param name="options">Json serializer settings.</param>
    /// <returns>The <see cref="AuditSettings"/>.</returns>
    public AuditSettingsBuilder WithJsonSerializerOptions(JsonSerializerOptions options)
    {
        _settings.JsonSerializerOptions = options;
        return this;
    }

    /// <summary>
    /// Sets whether only configured audited entities should be audited.
    /// </summary>
    /// <param name="onlyConfiguredAudited">Whether only configured audited entities should be audited.</param>
    /// <returns>The <see cref="AuditSettings"/>.</returns>
    public AuditSettingsBuilder OnlyConfiguredAudited(bool onlyConfiguredAudited = true)
	{
		_settings.OnlyConfiguredAudited = onlyConfiguredAudited;
		return this;
	}

    /// <summary>
    /// Builds the <see cref="AuditSettings"/>.
    /// </summary>
    /// <returns>The <see cref="AuditSettings"/>.</returns>
    internal AuditSettings Build()
    {
        return _settings;
    }
}