using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.DTOs
{
    public class QuotationRequestDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime RequiredByDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? ContactPhone { get; set; }
        public int ItemCount { get; set; }
        public int TotalItems { get; set; }
        public int ResponseCount { get; set; }
        public bool HasResponses { get; set; }
        public bool AlreadyResponded { get; set; }
        
        // ? ADDED: For distributor filtering logic
        public bool CanRespond { get; set; } = true;
        
        // ? ENHANCED: Add items collection for detailed views
        public List<QuotationRequestItemDTO> Items { get; set; } = new();
    }

    public class CreateQuotationRequestDTO
    {
        [Required]
        public int CustomerId { get; set; }

        public string? Notes { get; set; }

        public DateTime RequiredByDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? ContactPhone { get; set; }

        [Required]
        public List<CreateQuotationRequestItemDTO> Items { get; set; } = new();
    }

    public class CreateQuotationRequestItemDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public string? Specifications { get; set; }
    }

    public class QuotationRequestItemDTO
    {
        public int Id { get; set; }
        public int QuotationRequestId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductBrand { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public string? Specifications { get; set; }
    }

    public class QuotationResponseDTO
    {
        public int Id { get; set; }
        public int QuotationRequestId { get; set; }
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public string? DistributorEmail { get; set; }
        public string? DistributorPhone { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int ItemCount { get; set; }
        public int AverageDeliveryDays { get; set; }
        
        // ? ADDED: Items collection for detailed response view
        public List<QuotationResponseItemDTO> Items { get; set; } = new();
    }

    public class CreateQuotationResponseDTO
    {
        [Required]
        public int QuotationRequestId { get; set; }

        [Required]
        public int DistributorId { get; set; }

        public string? Notes { get; set; }

        [Required]
        public List<CreateQuotationResponseItemDTO> Items { get; set; } = new();
    }

    public class CreateQuotationResponseItemDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [Required]
        [Range(1, 365, ErrorMessage = "Delivery days must be between 1 and 365")]
        public int DeliveryDays { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class QuotationComparisonDTO
    {
        public int RequestId { get; set; }
        public bool Found { get; set; }
        public QuotationRequestDTO? QuotationRequest { get; set; }
        public List<QuotationResponseDTO> Responses { get; set; } = new();
        public List<QuotationRequestItemDTO> RequestItems { get; set; } = new();
        public decimal BestPrice { get; set; }
        public decimal WorstPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public int BestDelivery { get; set; }
        public int ResponseCount { get; set; }
    }

    public class QuotationStatsDTO
    {
        // Customer stats
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int CancelledRequests { get; set; }

        // Distributor stats
        public int TotalResponses { get; set; }
        public int AcceptedResponses { get; set; }
        public int RejectedResponses { get; set; }
        public int PendingResponses { get; set; }
        public decimal AverageResponseValue { get; set; }
        public decimal TotalResponseValue { get; set; }

        // Common stats
        public double AverageResponsesPerRequest { get; set; }
        public DateTime? LastRequestDate { get; set; }
        public DateTime? LastResponseDate { get; set; }
    }

    public class AcceptQuotationRequestDTO
    {
        [Required]
        public int ResponseId { get; set; }
        public int CustomerId { get; set; }
    }

    // ? ADDED: Missing QuotationResponseItemDTO class
    public class QuotationResponseItemDTO
    {
        public int Id { get; set; }
        public int QuotationResponseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductBrand { get; set; }
        public string? ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Stock { get; set; }
        public int DeliveryDays { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}