using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.DTOs
{
    public class CartSummaryDTO
    {
        public List<CartItemDTO> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CartItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string? Brand { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }
    }

    public class AddToCartDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        public string? SessionId { get; set; }
    }

    public class UpdateCartItemDTO
    {
        [Required]
        [Range(0, 100)] // 0 to remove item
        public int Quantity { get; set; }
    }

    public class MergeCartDTO
    {
        [Required]
        public string SessionId { get; set; } = string.Empty;
    }
}