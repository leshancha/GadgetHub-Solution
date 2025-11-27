namespace GadgetHubAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public string? SessionId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}