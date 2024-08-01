using System.Reflection;
using EF.AuditLog.Models;
using EF.AuditLog.Options;
using Microsoft.EntityFrameworkCore;

namespace EF.AuditLog.Extensions;

/// <summary>
/// Model builder extensions.
/// </summary>
public static class ModelBuilderExtensions
{
	private const string DefaultTableName = "AuditEntries";

	/// <summary>
	/// Apply audit configurations from assembly.
	/// </summary>
	/// <param name="modelBuilder">Model builder.</param>
	/// <param name="assembly">Assembly containing audit configurations.</param>
	/// <returns>Configured Model builder.</returns>
	public static ModelBuilder ApplyAuditConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly)
	{
		ConfigurationRegistry.RegisterConfigurationsFromAssembly(assembly);

		return modelBuilder;
	}

	/// <summary>
	/// Configure audit trail.
	/// </summary>
	/// <param name="modelBuilder">Model builder.</param>
	/// <param name="configureOptions">Configure settings.</param>
	/// <returns>Configured Model builder.</returns>
	public static ModelBuilder ApplyAuditSettings(this ModelBuilder modelBuilder, Action<AuditSettingsBuilder> configureOptions)
	{
	    var optionsBuilder = new AuditSettingsBuilder();

	    configureOptions(optionsBuilder);
	    var options = optionsBuilder.Build();

	    EntityAuditConfigurationsRegistry.SetAuditSettings(options);

	    return modelBuilder;
	}

	/// <summary>
	/// Configure audit trail.
	/// </summary>
	/// <param name="modelBuilder">Model builder.</param>
	/// <param name="tableName">Table name.</param>
	/// <returns>Configured Model builder.</returns>
	public static ModelBuilder ApplyAuditEntryConfiguration(this ModelBuilder modelBuilder, string tableName = DefaultTableName)
	{
		return ApplyAuditEntryConfiguration<AuditEntry>(modelBuilder, tableName);
	}

	/// <summary>
	/// Configure audit trail.
	/// </summary>
	/// <typeparam name="TEntity">Entity type.</typeparam>
	/// <param name="modelBuilder">Model builder.</param>
	/// <param name="tableName">Table name.</param>
	/// <returns>Configured Model builder.</returns>
	public static ModelBuilder ApplyAuditEntryConfiguration<TEntity>(this ModelBuilder modelBuilder, string tableName = DefaultTableName)
		where TEntity : AuditEntry
	{
		modelBuilder.Entity<TEntity>(
			builder =>
			{
				builder.ToTable(tableName);

				builder.HasKey(x => x.Id);

				builder.Property(x => x.TableName)
					.HasMaxLength(255)
					.IsRequired();

				builder.HasIndex(x => x.TableName);

				builder.Property(x => x.EntityId)
					.IsRequired();

				builder.HasIndex(x => x.EntityId);

				builder.Property(x => x.ModificationType)
					.HasConversion<string>();

				builder
					.Property(x => x.UpdatedValues)
					.IsRequired(false);
			});

		return modelBuilder;
	}
}