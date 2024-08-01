namespace EF.AuditLog.Samples.Models;

public sealed class Address
{
	public Address(string city, string? street, Building? building)
	{
		City = city;
		Street = street;
		Building = building;
	}

	private Address()
	{
	}

    /// <summary>
    /// City name.
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// Street name.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Building.
    /// </summary>
    public Building? Building { get; set; }
}