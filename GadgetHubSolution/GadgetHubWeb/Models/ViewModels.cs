using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GadgetHubWeb.Models.DTOs; // For CartItemDto, CategoryDto, etc.

namespace GadgetHubWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;
    }

    public class RegisterCustomerViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must agree to the terms")]
        [Display(Name = "I agree to the Terms of Service")]
        public bool AgreeToTerms { get; set; }
    }

    public class RegisterDistributorViewModel
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Business Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person is required")]
        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Business Phone")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Business Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Business License")]
        public string BusinessLicense { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must agree to the terms")]
        [Display(Name = "I agree to the Distributor Agreement")]
        public bool AgreeToTerms { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public List<ProductDto> RecommendedProducts { get; set; } = new();
        public bool IsAuthenticated { get; set; }
        public string? UserType { get; set; }
    }

    public class ProductSearchViewModel
    {
        public string? SearchQuery { get; set; }
        public int? CategoryId { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public List<ProductDto> Products { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public int TotalProducts { get; set; }
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public int UserId { get; set; }
        public List<OrderDto> RecentOrders { get; set; } = new();
        public List<QuotationRequestDto> RecentQuotations { get; set; } = new();
    }

    public class HomeViewModel
    {
        public List<ProductDto> FeaturedProducts { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public int TotalProductCount { get; set; }
        public List<ProductDto> NewArrivals { get; set; } = new();
        public List<ProductDto> BestSellers { get; set; } = new();
        public List<string> FeaturedBrands { get; set; } = new();
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
    }

    public class ProductsViewModel
    {
        public List<ProductDto> Products { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? CategoryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalProducts { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "name";
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string> AvailableBrands { get; set; } = new();
    }

    public class ProductDetailViewModel
    {
        public ProductDto Product { get; set; } = new();
        public List<ProductDto> RelatedProducts { get; set; } = new();
        public List<QuotationResponseDto> PriceComparisons { get; set; } = new();
        public bool IsInCart { get; set; }
        public int CartQuantity { get; set; }
        public List<ProductReviewDto> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public bool IsVerifiedPurchase { get; set; }
    }

    public class CreateQuotationRequestViewModel
    {
        public string Notes { get; set; } = string.Empty;
        public DateTime RequiredDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public List<QuotationItemRequest> Items { get; set; } = new();
        public string DeliveryAddress { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
    }

    public class QuotationItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Specifications { get; set; } = string.Empty;
    }

    public class CreateOrderViewModel
    {
        public int DistributorId { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public DateTime PreferredDeliveryDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CheckoutViewModel
    {
        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal => TotalAmount + ShippingCost + TaxAmount;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime PreferredDeliveryDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public string PaymentMethod { get; set; } = "CreditCard";
        public List<DistributorOption> DistributorOptions { get; set; } = new();
    }

    public class DistributorOption
    {
        public int DistributorId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public bool IsAvailable { get; set; }
    }

    // *** ADMIN VIEW MODELS ***
    public class AdminCustomersViewModel
    {
        public List<CustomerDto> Customers { get; set; } = new();
    }

    public class AdminDistributorsViewModel
    {
        public List<DistributorDto> Distributors { get; set; } = new();
    }

    public class AdminOrdersViewModel
    {
        public List<OrderDto> Orders { get; set; } = new();
    }

    public class AdminQuotationsViewModel
    {
        public List<QuotationRequestDto> QuotationRequests { get; set; } = new();
    }

    public class AdminReportsViewModel
    {
        public SystemReportDto? SystemReport { get; set; }
        public DashboardStatsDto? DashboardStats { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
