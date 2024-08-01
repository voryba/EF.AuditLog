using EF.AuditLog.Models;

namespace EF.AuditLog.Samples.Models;

public class LocalAuditEntity : AuditEntry
{
	/// <summary>
	/// Gets or sets the entity identifier.
	/// </summary>
    public string CreatedBy { get; set; }
}