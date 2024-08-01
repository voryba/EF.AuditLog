namespace EF.AuditLog.Samples.Models;

public sealed class Building
{
	public Building(string name, int number)
	{
		Name = name;
		Number = number;
	}

	private Building()
	{
	}

	/// <summary>
	/// Building name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Building number.
	/// </summary>
    public int Number { get; set; }
}