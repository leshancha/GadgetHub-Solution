using GadgetHubAPI.Data;
using GadgetHubAPI.DTOs;
using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IQuotationService _quotationService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, IOrderService orderService, IQuotationService quotationService, ILogger<AdminService> logger)
        {
            _context = context;
            _orderService = orderService;
            _quotationService = quotationService;
            _logger = logger;
        }

        public async Task<List<CustomerDTO>> GetAllCustomersAsync()
        {
            try
            {
                return await _context.Customers
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CustomerDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        Address = c.Address,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all customers at 2025-07-31 09:22:14 UTC by leshancha");
                return new List<CustomerDTO>();
            }
        }

        public async Task<List<DistributorDTO>> GetAllDistributorsAsync()
        {
            try
            {
                return await _context.Distributors
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => new DistributorDTO
                    {
                        Id = d.Id,
                        CompanyName = d.CompanyName,
                        Email = d.Email,
                        ContactPerson = d.ContactPerson,
                        Phone = d.Phone,
                        Address = d.Address,
                        IsActive = d.IsActive,
                        CreatedAt = d.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all distributors at 2025-07-31 09:22:14 UTC by leshancha");
                return new List<DistributorDTO>();
            }
        }

        public async Task<SystemOverviewDTO> GetSystemOverviewAsync()
        {
            try
            {
                var totalCustomers = await _context.Customers.CountAsync();
                var activeCustomers = await _context.Customers.CountAsync(c => c.IsActive);
                var totalDistributors = await _context.Distributors.CountAsync();
                var activeDistributors = await _context.Distributors.CountAsync(d => d.IsActive);
                var totalProducts = await _context.Products.CountAsync();
                var totalCategories = await _context.Categories.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();
                var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
                var totalQuotationRequests = await _context.QuotationRequests.CountAsync();
                var pendingQuotations = await _context.QuotationRequests.CountAsync(qr => qr.Status == "Pending");

                var recentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Distributor)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new RecentOrderDTO
                    {
                        OrderId = o.Id,
                        CustomerName = o.Customer.Name,
                        DistributorName = o.Distributor.CompanyName,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status
                    })
                    .ToListAsync();

                var recentQuotations = await _context.QuotationRequests
                    .Include(qr => qr.Customer)
                    .OrderByDescending(qr => qr.RequestDate)
                    .Take(5)
                    .Select(qr => new RecentQuotationDTO
                    {
                        RequestId = qr.Id,
                        CustomerName = qr.Customer.Name,
                        RequestDate = qr.RequestDate,
                        Status = qr.Status,
                        ResponseCount = qr.Responses.Count()
                    })
                    .ToListAsync();

                _logger.LogInformation($"System overview generated at 2025-07-31 09:22:14 UTC by leshancha");

                return new SystemOverviewDTO
                {
                    TotalCustomers = totalCustomers,
                    ActiveCustomers = activeCustomers,
                    TotalDistributors = totalDistributors,
                    ActiveDistributors = activeDistributors,
                    TotalProducts = totalProducts,
                    TotalCategories = totalCategories,
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    TotalQuotationRequests = totalQuotationRequests,
                    PendingQuotations = pendingQuotations,
                    RecentOrders = recentOrders,
                    RecentQuotations = recentQuotations,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating system overview at 2025-07-31 09:22:14 UTC by leshancha");
                return new SystemOverviewDTO { LastUpdated = DateTime.UtcNow };
            }
        }

        public async Task<bool> DeactivateCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null) return false;

                customer.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer {customerId} deactivated at 2025-07-31 09:22:14 UTC by leshancha");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating customer {customerId} at 2025-07-31 09:22:14 UTC by leshancha");
                return false;
            }
        }

        public async Task<bool> ActivateCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null) return false;

                customer.IsActive = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Customer {customerId} activated at 2025-07-31 09:22:14 UTC by leshancha");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error activating customer {customerId} at 2025-07-31 09:22:14 UTC by leshancha");
                return false;
            }
        }

        public async Task<bool> DeactivateDistributorAsync(int distributorId)
        {
            try
            {
                var distributor = await _context.Distributors.FindAsync(distributorId);
                if (distributor == null) return false;

                distributor.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Distributor {distributorId} deactivated at 2025-07-31 09:22:14 UTC by leshancha");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating distributor {distributorId} at 2025-07-31 09:22:14 UTC by leshancha");
                return false;
            }
        }

        public async Task<bool> ActivateDistributorAsync(int distributorId)
        {
            try
            {
                var distributor = await _context.Distributors.FindAsync(distributorId);
                if (distributor == null) return false;

                distributor.IsActive = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Distributor {distributorId} activated at 2025-07-31 09:22:14 UTC by leshancha");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error activating distributor {distributorId} at 2025-07-31 09:22:14 UTC by leshancha");
                return false;
            }
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            try
            {
                return await _orderService.GetAllOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all orders for admin at 2025-07-31 09:22:14 UTC by leshancha");
                return new List<OrderDTO>();
            }
        }

        public async Task<List<QuotationRequestDTO>> GetAllQuotationRequestsAsync()
        {
            try
            {
                return await _context.QuotationRequests
                    .Include(qr => qr.Customer)
                    .Include(qr => qr.Items)
                    .Include(qr => qr.Responses)
                    .OrderByDescending(qr => qr.RequestDate)
                    .Select(qr => new QuotationRequestDTO
                    {
                        Id = qr.Id,
                        CustomerId = qr.CustomerId,
                        CustomerName = qr.Customer.Name,
                        CustomerEmail = qr.Customer.Email,
                        RequestDate = qr.RequestDate,
                        Status = qr.Status,
                        Notes = qr.Notes,
                        ItemCount = qr.Items.Count,
                        TotalItems = qr.Items.Sum(qri => qri.Quantity),
                        ResponseCount = qr.Responses.Count,
                        HasResponses = qr.Responses.Any()
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all quotation requests for admin at 2025-07-31 09:22:14 UTC by leshancha");
                return new List<QuotationRequestDTO>();
            }
        }

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                var todayOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == today);
                var thisMonthOrders = await _context.Orders.CountAsync(o => o.OrderDate >= thisMonth);
                var lastMonthOrders = await _context.Orders.CountAsync(o => o.OrderDate >= lastMonth && o.OrderDate < thisMonth);

                var todayRevenue = await _context.Orders
                    .Where(o => o.OrderDate.Date == today && o.Status != "Cancelled")
                    .SumAsync(o => o.TotalAmount);

                var thisMonthRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= thisMonth && o.Status != "Cancelled")
                    .SumAsync(o => o.TotalAmount);

                var lastMonthRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= lastMonth && o.OrderDate < thisMonth && o.Status != "Cancelled")
                    .SumAsync(o => o.TotalAmount);

                var pendingQuotations = await _context.QuotationRequests.CountAsync(qr => qr.Status == "Pending");
                var activeCustomers = await _context.Customers.CountAsync(c => c.IsActive);
                var activeDistributors = await _context.Distributors.CountAsync(d => d.IsActive);

                var topProducts = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new TopProductDTO
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product.Name,
                        TotalSold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(tp => tp.TotalSold)
                    .Take(5)
                    .ToListAsync();

                return new DashboardStatsDTO
                {
                    TodayOrders = todayOrders,
                    ThisMonthOrders = thisMonthOrders,
                    LastMonthOrders = lastMonthOrders,
                    OrderGrowthPercentage = lastMonthOrders > 0 ? ((double)(thisMonthOrders - lastMonthOrders) / lastMonthOrders) * 100 : 0,

                    TodayRevenue = todayRevenue,
                    ThisMonthRevenue = thisMonthRevenue,
                    LastMonthRevenue = lastMonthRevenue,
                    RevenueGrowthPercentage = lastMonthRevenue > 0 ? ((double)(thisMonthRevenue - lastMonthRevenue) / (double)lastMonthRevenue) * 100 : 0,

                    PendingQuotations = pendingQuotations,
                    ActiveCustomers = activeCustomers,
                    ActiveDistributors = activeDistributors,
                    TopProducts = topProducts,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating dashboard stats at 2025-07-31 09:22:14 UTC by leshancha");
                return new DashboardStatsDTO { LastUpdated = DateTime.UtcNow };
            }
        }

        public async Task<AdminReportDTO> GenerateSystemReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
                var to = toDate ?? DateTime.UtcNow;

                var ordersInPeriod = await _context.Orders
                    .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                    .ToListAsync();

                var quotationsInPeriod = await _context.QuotationRequests
                    .Where(qr => qr.RequestDate >= from && qr.RequestDate <= to)
                    .ToListAsync();

                var newCustomers = await _context.Customers
                    .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
                    .CountAsync();

                var newDistributors = await _context.Distributors
                    .Where(d => d.CreatedAt >= from && d.CreatedAt <= to)
                    .CountAsync();

                return new AdminReportDTO
                {
                    FromDate = from,
                    ToDate = to,
                    TotalOrders = ordersInPeriod.Count,
                    TotalRevenue = ordersInPeriod.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount),
                    CompletedOrders = ordersInPeriod.Count(o => o.Status == "Delivered"),
                    CancelledOrders = ordersInPeriod.Count(o => o.Status == "Cancelled"),
                    TotalQuotations = quotationsInPeriod.Count,
                    CompletedQuotations = quotationsInPeriod.Count(qr => qr.Status == "Completed"),
                    NewCustomers = newCustomers,
                    NewDistributors = newDistributors,
                    AverageOrderValue = ordersInPeriod.Any() ? ordersInPeriod.Where(o => o.Status != "Cancelled").Average(o => o.TotalAmount) : 0,
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = "leshancha"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating system report at 2025-07-31 09:22:14 UTC by leshancha");
                return new AdminReportDTO
                {
                    FromDate = fromDate ?? DateTime.UtcNow.AddMonths(-1),
                    ToDate = toDate ?? DateTime.UtcNow,
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = "leshancha"
                };
            }
        }
    }
}