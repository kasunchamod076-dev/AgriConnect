using AgriConnect.Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using SupplierService.Models;

namespace SupplierService.Data;

// Tables and relationships owned by SupplierService. Other services use its API.
public class SupplierDbContext : DbContext
{
    public SupplierDbContext(DbContextOptions<SupplierDbContext> options)
        : base(options) { }

    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
    public DbSet<Quotation> Quotations { get; set; } = null!;
    public DbSet<Delivery> Deliveries { get; set; } = null!;

    // Use the account ID as the key of this service's local profile.
    public async Task EnsureProfileAsync(UserDto user)
    {
        var existing = await Suppliers.FindAsync(user.Id);
        if (existing == null)
        {
            Suppliers.Add(
                new Supplier
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    CompanyName = user.Name,
                    Address = user.District,
                }
            );
        }
        else
        {
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.CompanyName = user.Name;
            existing.Address = user.District;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Supplier>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder
            .Entity<Quotation>()
            .HasIndex(x => new { x.SupplyRequestId, x.SupplierId })
            .IsUnique();
        modelBuilder.Entity<Delivery>().HasIndex(x => x.SupplyRequestId).IsUnique();

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(s => s.Email).IsUnique();

            entity
                .HasMany(s => s.InventoryItems)
                .WithOne(i => i.Supplier)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(s => s.Quotations)
                .WithOne(q => q.Supplier)
                .HasForeignKey(q => q.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(s => s.Deliveries)
                .WithOne(d => d.Supplier)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.Property(q => q.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(q => q.TotalPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity
                .HasOne(d => d.Quotation)
                .WithMany()
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
