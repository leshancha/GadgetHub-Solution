using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GadgetHubAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int DistributorId { get; set; }
        public Distributor Distributor { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public string? Notes { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        // Navigation properties
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}