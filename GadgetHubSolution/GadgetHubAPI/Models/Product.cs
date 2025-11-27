using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<QuotationRequestItem> QuotationRequestItems { get; set; } = new List<QuotationRequestItem>();
        public ICollection<QuotationResponseItem> QuotationResponseItems { get; set; } = new List<QuotationResponseItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<DistributorInventory> DistributorInventories { get; set; } = new List<DistributorInventory>();
    }
}