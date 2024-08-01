using EF.AuditLog.Models;
using EF.AuditLog.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EF.AuditLog.Interceptors;

/// <summary>
/// Represents an interceptor for auditing entity changes.
/// </summary>
/// <typeparam name="TAuditEntry">The type of the audit entry.</typeparam>
public abstract class AuditSaveChangesInterceptorBase<TAuditEntry> : SaveChangesInterceptor
    where TAuditEntry : AuditEntry
{
    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            eventData.Context!.AuditEntries(createHistoryFactory: AuditEntryFactory());
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Creates an audit entry factory.
    /// </summary>
    /// <typeparam name="TAuditEntry">The type of the audit entry.</typeparam>
    /// <returns>The audit entry factory.</returns>
    protected abstract Func<TAuditEntry> AuditEntryFactory();
}