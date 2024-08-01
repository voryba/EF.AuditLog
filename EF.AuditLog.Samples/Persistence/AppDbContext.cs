using EF.AuditLog.Samples.Models;
using EF.AuditLog.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EF.AuditLog.Samples.Persistence;

internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<User>().OwnsOne(x => x.Address, x =>
        {
            x.OwnsOne(r => r.Building);
        });

        modelBuilder.Entity<UserRole>().HasKey(x => x.Id);

        modelBuilder
            .ApplyAuditEntryConfiguration<LocalAuditEntity>()
            .ApplyAuditConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
}