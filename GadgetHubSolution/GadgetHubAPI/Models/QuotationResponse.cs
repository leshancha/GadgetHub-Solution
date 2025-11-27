using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GadgetHubAPI.Models
{
    public class QuotationResponse
    {
        public int Id { get; set; }

        public int QuotationRequestId { get; set; }
        public QuotationRequest QuotationRequest { get; set; } = null!;

        public int DistributorId { get; set; }
        public Distributor Distributor { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted";

        public string? Notes { get; set; }

        // Navigation properties
        public ICollection<QuotationResponseItem> Items { get; set; } = new List<QuotationResponseItem>();
    }
}