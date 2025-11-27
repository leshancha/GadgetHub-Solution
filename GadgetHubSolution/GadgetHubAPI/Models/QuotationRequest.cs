using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Models
{
    public class QuotationRequest
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public string? Notes { get; set; }

        // Additional fields for enhanced functionality
        public DateTime RequiredByDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? ContactPhone { get; set; }

        // Navigation properties
        public ICollection<QuotationRequestItem> Items { get; set; } = new List<QuotationRequestItem>();
        public ICollection<QuotationResponse> Responses { get; set; } = new List<QuotationResponse>();
    }
}