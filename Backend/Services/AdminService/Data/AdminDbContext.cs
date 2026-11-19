using AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Data;

// Tables and relationships owned by AdminService. Other services use its API.
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options) { }

    public DbSet<PasswordRecovery> PasswordRecoveries => Set<PasswordRecovery>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    // Save the account change and its audit entry in the same transaction.
    public void AddActivity(int? userId, string action, string detail)
    {
        AuditLogs.Add(
            new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = "Accounts",
                Details = detail,
            }
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // The user ID is both the primary key and the foreign key; there is no second user ID.
        modelBuilder
            .Entity<PasswordRecovery>()
            .HasOne(r => r.User)
            .WithOne()
            .HasForeignKey<PasswordRecovery>(r => r.UserId);
        modelBuilder.Entity<Session>().HasIndex(s => s.UserId);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed default roles
        modelBuilder
            .Entity<Role>()
            .HasData(
                new Role
                {
                    Id = 1,
                    Name = "Administrator",
                    Description = "System Administrator",
                },
                new Role
                {
                    Id = 2,
                    Name = "Farmer",
                    Description = "Farmer Account",
                },
                new Role
                {
                    Id = 3,
                    Name = "Supplier",
                    Description = "Agricultural Supplier",
                },
                new Role
                {
                    Id = 4,
                    Name = "AgricultureOfficer",
                    Description = "Government Agriculture Officer",
                }
            );
    }
}
