using System.ComponentModel.DataAnnotations.Schema;

namespace EF.AuditLog.Samples.Models;

public sealed class User
{
	/// <summary>
	/// User identifier.
	/// </summary>
    public Guid Id { get; set; }

	/// <summary>
	/// User name.
	/// </summary>
    public string Name { get; set; }

	/// <summary>
	/// User email.
	/// </summary>
    public string Password { get; set; }

	/// <summary>
	/// Sensitive data.
	/// </summary>
    public string? AdditionalInformation { get; set; }

	/// <summary>
	/// User Car identifier.
	/// </summary>
    public Guid CarId { get; set; }

	/// <summary>
	/// User Car.
	/// </summary>
    [ForeignKey(nameof(CarId))]
    public Car? Car { get; set; }

    /// <summary>
    /// User address.
    /// </summary>
    public Address? Address { get; set; }

    /// <summary>
    /// User roles.
    /// </summary>
    public List<UserRole> Roles { get; set; }
}