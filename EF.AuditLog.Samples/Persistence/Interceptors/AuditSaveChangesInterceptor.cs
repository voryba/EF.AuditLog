using EF.AuditLog.Samples.Models;
using EF.AuditLog.Interceptors;

namespace EF.AuditLog.Samples.Persistence.Interceptors;

internal sealed class AuditSaveChangesInterceptor : AuditSaveChangesInterceptorBase<LocalAuditEntity>
{
    protected override Func<LocalAuditEntity> AuditEntryFactory()
    {
        return () => new LocalAuditEntity()
        {
            CreatedBy = "test-user-created",
        };
    }
}