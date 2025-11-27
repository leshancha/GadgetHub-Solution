using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GadgetHubAPI.Models
{
    public class DistributorInventory
    {
        public int Id { get; set; }

        public int DistributorId { get; set; }
        public Distributor Distributor { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public int DeliveryDays { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}