using System.ComponentModel.DataAnnotations;

namespace GadgetHubWeb.Models.DTOs
{
    // Auth DTOs
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class RegisterCustomerRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class RegisterDistributorRequest
    {
        [Required(ErrorMessage = "Company name is required")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person is required")]
        public string ContactPerson { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    // Product DTOs
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        // Add missing properties for API usage
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int AvailableStock { get; set; }
        public int DistributorCount { get; set; }
        public bool Success { get; set; }
        public object? Data { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        // Add missing properties for API usage
        public bool Success { get; set; }
        public object? Data { get; set; }
    }

    // Cart DTOs
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductBrand { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        // Add missing property for estimated price
        public decimal EstimatedPrice { get; set; }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    // Order DTOs
    public class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public int DistributorId { get; set; }
        public string? DistributorName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public int ItemCount { get; set; }
        public int TotalItems { get; set; }
    }

    public class CreateOrderRequest
    {
        public int DistributorId { get; set; }
        public string? Notes { get; set; }
        public int? EstimatedDeliveryDays { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Quotation DTOs
    public class QuotationRequestDto
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
        public int ItemCount { get; set; }
        public int TotalItems { get; set; }
        public int ResponseCount { get; set; }
        public bool HasResponses { get; set; }
        public bool AlreadyResponded { get; set; }
    }

    public class QuotationResponseDto
    {
        public int Id { get; set; }
        public int QuotationRequestId { get; set; }
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public string DistributorEmail { get; set; } = string.Empty;
        public string DistributorPhone { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int ItemCount { get; set; }
        public int AverageDeliveryDays { get; set; }
        public List<QuotationResponseItemDto> Items { get; set; } = new();
    }

    public class QuotationComparisonDto
    {
        public int RequestId { get; set; }
        public bool Found { get; set; }
        public QuotationRequestDto? QuotationRequest { get; set; }
        public List<QuotationResponseDto> Responses { get; set; } = new();
        public decimal BestPrice { get; set; }
        public decimal WorstPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public int BestDelivery { get; set; }
        public int ResponseCount { get; set; }
    }

    public class CreateQuotationRequest
    {
        public string? Notes { get; set; }
        public List<CreateQuotationItemRequest> Items { get; set; } = new();
    }

    public class CreateQuotationItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Admin DTOs
    public class DashboardStatsDto
    {
        public int TodayOrders { get; set; }
        public int ThisMonthOrders { get; set; }
        public int LastMonthOrders { get; set; }
        public double OrderGrowthPercentage { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public double RevenueGrowthPercentage { get; set; }
        public int PendingQuotations { get; set; }
        public int ActiveCustomers { get; set; }
        public int ActiveDistributors { get; set; }
        public List<TopProductDto> TopProducts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // Session Cart (for guests)
    public class SessionCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductBrand { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    // API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class QuotationItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Specifications { get; set; } = string.Empty;
    }

    public class CustomerRegistrationRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class DistributorRegistrationRequest
    {
        [Required(ErrorMessage = "Company name is required")]
        public string CompanyName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "Contact person is required")]
        public string ContactPerson { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    // Distributor Inventory DTOs
    public class DistributorInventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductBrand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class UpdateInventoryRequest
    {
        public int DistributorId { get; set; } = 1;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ✅ ADDED: Quotation request details DTO
    public class QuotationRequestDetailsDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime RequiredByDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? ContactPhone { get; set; }
        public List<QuotationRequestItemDetailsDto> Items { get; set; } = new();
    }

    public class QuotationRequestItemDetailsDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Specifications { get; set; }
    }

    // ✅ NEW: Additional DTOs for enhanced quotation functionality
    public class QuotationRequestItemDto
    {
        public int Id { get; set; }
        public int QuotationRequestId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductBrand { get; set; } = string.Empty;
        public string ProductCategory { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Specifications { get; set; } = string.Empty;
    }

    public class QuotationResponseItemDto
    {
        public int Id { get; set; }
        public int QuotationResponseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductBrand { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Stock { get; set; }
        public int DeliveryDays { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // ✅ NEW: API-specific response models for parsing API responses
    public class ApiProductsResponse
    {
        public List<ApiProduct> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ApiProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public ApiCategory Category { get; set; } = new();
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int AvailableStock { get; set; }
        public int DistributorCount { get; set; }
    }

    public class ApiProductDetail : ApiProduct
    {
        public List<ApiPricing> Pricing { get; set; } = new();
    }

    public class ApiCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ApiPricing
    {
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int DeliveryDays { get; set; }
    }

    public class CartApiResponse
    {
        public List<ApiCartItem> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ApiCartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductBrand { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal EstimatedPrice { get; set; }
    }

    public class ApiQuotationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }

    // ✅ ADMIN DASHBOARD DTOS
    public class AdminOverviewDto
    {
        public int TotalCustomers { get; set; }
        public int TotalDistributors { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveQuotations { get; set; }
        public int Categories { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = "admin";
    }

    public class SystemReportDto
    {
        public SystemHealthDto SystemHealth { get; set; } = new();
        public PerformanceDto Performance { get; set; } = new();
        public UsageDto Usage { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = "admin";
    }

    public class SystemHealthDto
    {
        public bool DatabaseConnected { get; set; } = true;
        public int TotalTables { get; set; } = 10;
        public DateTime LastBackup { get; set; }
        public string Status { get; set; } = "Healthy";
    }

    public class PerformanceDto
    {
        public string AverageResponseTime { get; set; } = "150ms";
        public int TotalRequests { get; set; } = 1250;
        public string ErrorRate { get; set; } = "0.2%";
        public string Uptime { get; set; } = "99.8%";
    }

    public class UsageDto
    {
        public int ActiveUsers { get; set; }
        public int DailyTransactions { get; set; }
        public string StorageUsed { get; set; } = "2.4 GB";
        public int ApiCalls { get; set; } = 8750;
    }

    // ✅ CUSTOMER AND DISTRIBUTOR DTOS
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class DistributorDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
    }
}