using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public int DistributorId { get; set; }
        public string? DistributorName { get; set; }
        public string? DistributorEmail { get; set; }
        public string? DistributorPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public int ItemCount { get; set; }
        public int TotalItems { get; set; }
    }

    public class CreateOrderDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int DistributorId { get; set; }

        public string? Notes { get; set; }

        public int? EstimatedDeliveryDays { get; set; }

        [Required]
        public List<CreateOrderItemDTO> Items { get; set; } = new();
    }

    public class CreateOrderItemDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductBrand { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderSummaryDTO
    {
        public int OrderId { get; set; }
        public bool Found { get; set; }
        public OrderDTO? Order { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new();
        public object? Summary { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}