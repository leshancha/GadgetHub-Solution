using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Models
{
    public class Distributor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties - ALL REQUIRED FOR ENTITY FRAMEWORK
        public ICollection<DistributorInventory> Inventories { get; set; } = new List<DistributorInventory>();
        public ICollection<QuotationResponse> QuotationResponses { get; set; } = new List<QuotationResponse>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}