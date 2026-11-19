using AgriConnect.Contracts.DTOs;
using FarmerService.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerService.Data;

// Tables and relationships owned by FarmerService. Other services use its API.
public class FarmerDbContext : DbContext
{
    public FarmerDbContext(DbContextOptions<FarmerDbContext> options)
        : base(options) { }

    public DbSet<Farmer> Farmers { get; set; } = null!;

    public DbSet<SupplyRequest> SupplyRequests { get; set; } = null!;

    public DbSet<CropListing> CropListings { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;

    // Use the account ID as the key of this service's local profile.
    public async Task EnsureProfileAsync(UserDto user)
    {
        var existing = await Farmers.FindAsync(user.Id);
        if (existing == null)
        {
            Farmers.Add(
                new Farmer
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.Name,
                    Location = user.District,
                }
            );
        }
        else
        {
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.FullName = user.Name;
            existing.Location = user.District;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Farmer>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Order>().HasIndex(x => x.SupplyRequestId).IsUnique();
        modelBuilder.Entity<SupplyRequest>().HasIndex(x => x.District);

        // Farmer configuration
        modelBuilder.Entity<Farmer>(entity =>
        {
            entity.HasIndex(f => f.Email).IsUnique();

            entity
                .HasMany(f => f.SupplyRequests)
                .WithOne(sr => sr.Farmer)
                .HasForeignKey(sr => sr.FarmerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(f => f.CropListings)
                .WithOne(cl => cl.Farmer)
                .HasForeignKey(cl => cl.FarmerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SupplyRequest configuration
        modelBuilder.Entity<SupplyRequest>(entity =>
        {
            entity
                .HasMany(sr => sr.Orders)
                .WithOne(o => o.SupplyRequest)
                .HasForeignKey(o => o.SupplyRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity
                .HasOne(o => o.Farmer)
                .WithMany()
                .HasForeignKey(o => o.FarmerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        });

        // CropListing configuration
        modelBuilder.Entity<CropListing>(entity =>
        {
            entity.Property(cl => cl.Quantity).HasColumnType("decimal(18,2)");

            entity.Property(cl => cl.PricePerUnit).HasColumnType("decimal(18,2)");
        });
    }
}
