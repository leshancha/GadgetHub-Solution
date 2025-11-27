using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.Models
{
    public class QuotationRequestItem
    {
        public int Id { get; set; }

        public int QuotationRequestId { get; set; }
        public QuotationRequest QuotationRequest { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public string? Specifications { get; set; }
    }
}