using AgriConnect.Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using OfficerService.Models;

namespace OfficerService.Data;

// Tables and relationships owned by OfficerService. Other services use its API.
public class OfficerDbContext : DbContext
{
    public OfficerDbContext(DbContextOptions<OfficerDbContext> options)
        : base(options) { }

    public DbSet<Officer> Officers { get; set; } = null!;
    public DbSet<Verification> Verifications { get; set; } = null!;
    public DbSet<Recommendation> Recommendations { get; set; } = null!;

    // Use the account ID as the key of this service's local profile.
    public async Task EnsureProfileAsync(UserDto user)
    {
        var existing = await Officers.FindAsync(user.Id);
        if (existing == null)
        {
            Officers.Add(
                new Officer
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.Name,
                    Region = user.District,
                }
            );
        }
        else
        {
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.FullName = user.Name;
            existing.Region = user.District;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Officer>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Verification>().HasIndex(x => x.SupplyRequestId).IsUnique();

        modelBuilder.Entity<Officer>(entity =>
        {
            entity.HasIndex(o => o.Email).IsUnique();

            entity
                .HasMany(o => o.Verifications)
                .WithOne(v => v.Officer)
                .HasForeignKey(v => v.OfficerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(o => o.Recommendations)
                .WithOne(r => r.Officer)
                .HasForeignKey(r => r.OfficerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
