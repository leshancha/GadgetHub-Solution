using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties - THESE WERE MISSING
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<QuotationRequest> QuotationRequests { get; set; } = new List<QuotationRequest>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}