namespace EF.AuditLog.Samples.Models;

public sealed class Car
{
	public Car(string name)
	{
		Name = name;
	}

	private Car()
	{
	}

	public Guid Id { get; set; }
	public string Name { get; set; }
}