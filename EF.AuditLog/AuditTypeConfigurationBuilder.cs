using System.Linq.Expressions;
using EF.AuditLog.Options;

namespace EF.AuditLog;

/// <summary>
/// Audit type builder.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public class AuditTypeConfigurationBuilder<TEntity>
    where TEntity : class
{
    private readonly List<string> _ignoredProperties = new();
    private bool _excludeEntity;

    /// <summary>
    /// Excluded properties from audit.
    /// </summary>
    /// <param name="propertyExpression">Property expression.</param>
    /// <typeparam name="TProperty">Property type.</typeparam>
    /// <returns>Audit type builder.</returns>
    public AuditTypeConfigurationBuilder<TEntity> IgnoreProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
        _ignoredProperties.Add(propertyName);
        return this;
    }

    /// <summary>
    /// Exclude entity from audit.
    /// </summary>
    public void ExcludeEntity()
    {
        _excludeEntity = true;
    }

    /// <summary>
    /// Build the audit type configuration.
    /// </summary>
    /// <returns>The audit type configuration.</returns>
    internal EntityAuditConfiguration Build()
    {
        return new EntityAuditConfiguration(_ignoredProperties, _excludeEntity);
    }
}