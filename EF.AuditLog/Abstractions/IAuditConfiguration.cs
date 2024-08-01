namespace EF.AuditLog.Abstractions;

/// <summary>
/// Audit configuration interface.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface IAuditConfiguration<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Configure audit.
    /// </summary>
    /// <param name="configurationBuilder">Audit type configurationBuilder.</param>
    void Configure(AuditTypeConfigurationBuilder<TEntity> configurationBuilder);
}