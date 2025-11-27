using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateProductDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }
    }

    public class UpdateProductDTO
    {
        [StringLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }
    }

    public class ProductSearchDTO
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? SortBy { get; set; } = "name";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class ProductInventoryDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public List<DistributorInventoryDTO> DistributorInventories { get; set; } = new();
    }

    public class DistributorInventoryDTO
    {
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsActive { get; set; }
    }
}