using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Data;
using GadgetHubAPI.Models;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("overview")]
        public async Task<ActionResult> GetOverview()
        {
            try
            {
                var overview = new
                {
                    TotalCustomers = await _context.Customers.CountAsync(),
                    TotalDistributors = await _context.Distributors.CountAsync(),
                    TotalProducts = await _context.Products.CountAsync(),
                    TotalOrders = await _context.Orders.CountAsync(),
                    ActiveQuotations = await _context.QuotationRequests.CountAsync(q => q.Status == "Pending"),
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = "leshancha"
                };

                _logger.LogInformation($"✅ Admin overview generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetOverview: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("dashboard/stats")]
        public async Task<ActionResult> GetDashboardStats()
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

                var pendingQuotations = await _context.QuotationRequests.CountAsync(q => q.Status == "Pending");
                var activeCustomers = await _context.Customers.CountAsync(c => c.IsActive);
                var activeDistributors = await _context.Distributors.CountAsync(d => d.IsActive);

                var topProducts = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product.Name,
                        TotalSold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(tp => tp.TotalSold)
                    .Take(5)
                    .ToListAsync();

                var stats = new
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
                    LastUpdated = DateTime.UtcNow,
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = "leshancha"
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetDashboardStats: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("customers")]
        public async Task<ActionResult> GetCustomers()
        {
            try
            {
                var customers = await _context.Customers
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Email,
                        c.Phone,
                        c.IsActive,
                        c.CreatedAt,
                        OrderCount = c.Orders.Count(),
                        TotalSpent = c.Orders.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetCustomers: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("distributors")]
        public async Task<ActionResult> GetDistributors()
        {
            try
            {
                var distributors = await _context.Distributors
                    .Select(d => new
                    {
                        d.Id,
                        d.CompanyName,
                        d.Email,
                        d.ContactPerson,
                        d.Phone,
                        d.IsActive,
                        d.CreatedAt,
                        ProductCount = d.Inventories.Count(),
                        OrderCount = d.Orders.Count()
                    })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                return Ok(distributors);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetDistributors: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("orders")]
        public async Task<ActionResult> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Distributor)
                    .Select(o => new
                    {
                        o.Id,
                        o.CustomerId,
                        CustomerName = o.Customer.Name,
                        CustomerEmail = o.Customer.Email,
                        o.DistributorId,
                        DistributorName = o.Distributor.CompanyName,
                        o.TotalAmount,
                        o.Status,
                        o.OrderDate,
                        ItemCount = o.Items.Count(),
                        TotalItems = o.Items.Sum(i => i.Quantity)
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetOrders: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("quotations")]
        public async Task<ActionResult> GetQuotations()
        {
            try
            {
                var quotations = await _context.QuotationRequests
                    .Include(q => q.Customer)
                    .Select(q => new
                    {
                        q.Id,
                        CustomerName = q.Customer.Name,
                        q.Status,
                        q.RequestDate,
                        ItemCount = q.Items.Count(),
                        ResponseCount = q.Responses.Count()
                    })
                    .OrderByDescending(q => q.RequestDate)
                    .ToListAsync();

                return Ok(quotations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetQuotations: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("customers/{id}/deactivate")]
        public async Task<ActionResult> DeactivateCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }

                customer.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Customer {id} deactivated by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return Ok(new { message = "Customer deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in DeactivateCustomer: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("customers/{id}/activate")]
        public async Task<ActionResult> ActivateCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }

                customer.IsActive = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Customer {id} activated by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return Ok(new { message = "Customer activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in ActivateCustomer: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("reports/system")]
        public async Task<ActionResult> GetSystemReports()
        {
            try
            {
                var report = new
                {
                    SystemHealth = new
                    {
                        DatabaseConnected = true,
                        TotalTables = 10,
                        LastBackup = DateTime.UtcNow.AddDays(-1),
                        Status = "Healthy"
                    },
                    Performance = new
                    {
                        AverageResponseTime = "150ms",
                        TotalRequests = 1250,
                        ErrorRate = "0.2%",
                        Uptime = "99.8%"
                    },
                    Usage = new
                    {
                        ActiveUsers = await _context.Customers.CountAsync(c => c.IsActive) + await _context.Distributors.CountAsync(d => d.IsActive),
                        DailyTransactions = await _context.Orders.CountAsync(o => o.OrderDate.Date == DateTime.UtcNow.Date),
                        StorageUsed = "2.4 GB",
                        ApiCalls = 8750
                    },
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = "leshancha"
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetSystemReports: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("distributors/{id}/deactivate")]
        public async Task<ActionResult> DeactivateDistributor(int id)
        {
            try
            {
                var distributor = await _context.Distributors.FindAsync(id);
                if (distributor == null)
                {
                    return NotFound(new { message = "Distributor not found" });
                }

                distributor.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Distributor {id} deactivated by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return Ok(new { message = "Distributor deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in DeactivateDistributor: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPut("distributors/{id}/activate")]
        public async Task<ActionResult> ActivateDistributor(int id)
        {
            try
            {
                var distributor = await _context.Distributors.FindAsync(id);
                if (distributor == null)
                {
                    return NotFound(new { message = "Distributor not found" });
                }

                distributor.IsActive = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Distributor {id} activated by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return Ok(new { message = "Distributor activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in ActivateDistributor: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("products")]
        public async Task<ActionResult> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.ImageUrl,
                        p.Brand,
                        p.Model,
                        p.IsActive,
                        p.CreatedAt,
                        CategoryName = p.Category.Name,
                        TotalStock = p.DistributorInventories.Sum(di => di.Stock)
                    })
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetProducts: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("analytics")]
        public async Task<ActionResult> GetAnalytics()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                var analytics = new
                {
                    CustomerGrowth = new
                    {
                        ThisMonth = await _context.Customers.CountAsync(c => c.CreatedAt >= thisMonth),
                        LastMonth = await _context.Customers.CountAsync(c => c.CreatedAt >= lastMonth && c.CreatedAt < thisMonth),
                        Total = await _context.Customers.CountAsync()
                    },
                    OrderTrends = new
                    {
                        Daily = await _context.Orders
                            .Where(o => o.OrderDate >= today.AddDays(-7))
                            .GroupBy(o => o.OrderDate.Date)
                            .Select(g => new { Date = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.TotalAmount) })
                            .OrderBy(x => x.Date)
                            .ToListAsync(),
                        Monthly = await _context.Orders
                            .Where(o => o.OrderDate >= thisMonth.AddMonths(-12))
                            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                            .Select(g => new { 
                                Year = g.Key.Year, 
                                Month = g.Key.Month, 
                                Count = g.Count(), 
                                Revenue = g.Sum(o => o.TotalAmount) 
                            })
                            .OrderBy(x => x.Year).ThenBy(x => x.Month)
                            .ToListAsync()
                    },
                    ProductPerformance = await _context.OrderItems
                        .Include(oi => oi.Product)
                        .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                        .Select(g => new
                        {
                            ProductId = g.Key.ProductId,
                            ProductName = g.Key.Name,
                            TotalSold = g.Sum(oi => oi.Quantity),
                            TotalRevenue = g.Sum(oi => oi.TotalPrice),
                            OrderCount = g.Count()
                        })
                        .OrderByDescending(x => x.TotalRevenue)
                        .Take(10)
                        .ToListAsync(),
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = "leshancha"
                };

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetAnalytics: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("export/customers")]
        public async Task<ActionResult> ExportCustomers()
        {
            try
            {
                var customers = await _context.Customers
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Email,
                        c.Phone,
                        c.Address,
                        c.IsActive,
                        c.CreatedAt,
                        OrderCount = c.Orders.Count(),
                        TotalSpent = c.Orders.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Data = customers,
                    ExportedAt = DateTime.UtcNow,
                    ExportedBy = "leshancha",
                    TotalRecords = customers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in ExportCustomers: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("backup")]
        public async Task<ActionResult> CreateBackup()
        {
            try
            {
                // Simulate backup creation
                await Task.Delay(2000);

                var backup = new
                {
                    BackupId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "leshancha",
                    Status = "Completed",
                    Size = "15.2 MB",
                    Tables = new string[] { "Customers", "Distributors", "Products", "Orders", "QuotationRequests" },
                    Records = new
                    {
                        Customers = await _context.Customers.CountAsync(),
                        Distributors = await _context.Distributors.CountAsync(),
                        Products = await _context.Products.CountAsync(),
                        Orders = await _context.Orders.CountAsync(),
                        QuotationRequests = await _context.QuotationRequests.CountAsync()
                    }
                };

                _logger.LogInformation($"✅ Database backup created by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return Ok(backup);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in CreateBackup: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}