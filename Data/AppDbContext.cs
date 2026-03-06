using Microsoft.EntityFrameworkCore;
using FionetixAPI.Models;

namespace FionetixAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Spouse> Spouses => Set<Spouse>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NID).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.NID).IsUnique();
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // Spouse configuration — One-to-One with Employee
        modelBuilder.Entity<Spouse>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.NID).HasMaxLength(20);
            entity.HasIndex(s => s.EmployeeId).IsUnique();
            entity.HasOne(s => s.Employee)
                  .WithOne(e => e.Spouse)
                  .HasForeignKey<Spouse>(s => s.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Child configuration — One-to-Many with Employee
        modelBuilder.Entity<Child>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(c => c.Employee)
                  .WithMany(e => e.Children)
                  .HasForeignKey(c => c.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AppUser configuration
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FirebaseUid).IsRequired().HasMaxLength(128);
            entity.HasIndex(u => u.FirebaseUid).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(10);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Employee>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
