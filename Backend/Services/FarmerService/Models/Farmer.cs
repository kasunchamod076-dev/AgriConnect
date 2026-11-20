using System.ComponentModel.DataAnnotations;

namespace FarmerService.Models;

// Farmer-side profile used by owned requests and harvest listings.
public class Farmer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    public List<SupplyRequest> SupplyRequests { get; set; } = new();

    public List<CropListing> CropListings { get; set; } = new();
}