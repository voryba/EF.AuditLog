using EF.AuditLog.Models;

namespace EF.AuditLog.Interceptors;

/// <summary>
/// Represents an interceptor for auditing entity changes.
/// </summary>
public class AuditSaveChangesInterceptor : AuditSaveChangesInterceptorBase<AuditEntry>
{
    /// <summary>
    /// Creates an audit entry factory.
    /// </summary>
    /// <returns>The audit entry factory.</returns>
    protected override Func<AuditEntry> AuditEntryFactory() => () => new AuditEntry();
}