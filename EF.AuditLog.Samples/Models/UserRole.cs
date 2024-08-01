using System.ComponentModel.DataAnnotations.Schema;

namespace EF.AuditLog.Samples.Models;

public class UserRole
{
	/// <summary>
	/// Gets or sets the role identifier.
	/// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// User identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

}