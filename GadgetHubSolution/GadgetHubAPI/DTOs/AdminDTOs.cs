using System.ComponentModel.DataAnnotations;

namespace GadgetHubAPI.DTOs
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DistributorDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SystemOverviewDTO
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int TotalDistributors { get; set; }
        public int ActiveDistributors { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalQuotationRequests { get; set; }
        public int PendingQuotations { get; set; }
        public List<RecentOrderDTO> RecentOrders { get; set; } = new();
        public List<RecentQuotationDTO> RecentQuotations { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class RecentOrderDTO
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string DistributorName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RecentQuotationDTO
    {
        public int RequestId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
    }

    public class DashboardStatsDTO
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
        public List<TopProductDTO> TopProducts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class TopProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class AdminReportDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int TotalQuotations { get; set; }
        public int CompletedQuotations { get; set; }
        public int NewCustomers { get; set; }
        public int NewDistributors { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }

    public class UserStatusUpdateDTO
    {
        [Required]
        public bool IsActive { get; set; }

        public string? Reason { get; set; }
    }
}