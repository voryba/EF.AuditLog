using EF.AuditLog.Extensions;
using EF.AuditLog.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace EF.AuditLog.Tests;

public sealed class AuditDbContext : DbContext
{
	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder
			.ApplyAuditEntryConfiguration()
			.ApplyAuditConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
	}

	/// <inheritdoc/>
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder
			.AddInterceptors(new AuditSaveChangesInterceptor())
			.UseInMemoryDatabase(nameof(AuditDbContext));
	}
}