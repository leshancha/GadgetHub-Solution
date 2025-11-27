using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Data;
using GadgetHubAPI.Models;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistributorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DistributorController> _logger;

        public DistributorController(ApplicationDbContext context, ILogger<DistributorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("inventory")]
        public async Task<ActionResult> GetInventory([FromQuery] int distributorId = 1)
        {
            try
            {
                _logger.LogInformation($"📦 GetInventory called for distributorId={distributorId}");

                // ✅ ENHANCED: Get comprehensive inventory data with detailed product information
                var inventory = await _context.DistributorInventories
                    .Include(di => di.Product)
                        .ThenInclude(p => p.Category)
                    .Where(di => di.DistributorId == distributorId)
                    .Select(di => new
                    {
                        di.Id,
                        di.ProductId,
                        ProductName = di.Product.Name,
                        ProductBrand = di.Product.Brand ?? "Unknown",
                        ProductModel = di.Product.Model ?? "",
                        ProductDescription = di.Product.Description ?? "",
                        ProductImage = di.Product.ImageUrl ?? "https://via.placeholder.com/50x50?text=IMG",
                        Category = di.Product.Category.Name,
                        di.Price,
                        di.Stock,
                        di.DeliveryDays,
                        di.IsActive,
                        di.LastUpdated,
                        // ✅ ADDED: Calculate stock status
                        StockStatus = di.Stock > 10 ? "In Stock" : di.Stock > 0 ? "Low Stock" : "Out of Stock",
                        StockLevel = di.Stock > 10 ? "high" : di.Stock > 0 ? "medium" : "low"
                    })
                    .OrderBy(di => di.ProductName)
                    .ToListAsync();

                _logger.LogInformation($"✅ Found {inventory.Count} inventory items for distributor {distributorId}");

                // ✅ ENHANCED: Return success response with metadata
                return Ok(new
                {
                    success = true,
                    data = inventory,
                    count = inventory.Count,
                    activeCount = inventory.Count(i => i.IsActive),
                    inStockCount = inventory.Count(i => i.Stock > 0),
                    retrievedAt = DateTime.UtcNow,
                    distributorId = distributorId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetInventory: {ex.Message}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("inventory/{productId}")]
        public async Task<ActionResult> GetInventoryItem(int productId, [FromQuery] int distributorId = 1)
        {
            try
            {
                var item = await _context.DistributorInventories
                    .Include(di => di.Product)
                        .ThenInclude(p => p.Category)
                    .Where(di => di.ProductId == productId && di.DistributorId == distributorId)
                    .Select(di => new
                    {
                        di.Id,
                        di.ProductId,
                        ProductName = di.Product.Name,
                        ProductBrand = di.Product.Brand,
                        Category = di.Product.Category.Name,
                        di.Price,
                        di.Stock,
                        di.DeliveryDays,
                        di.IsActive,
                        di.LastUpdated
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { 
                        success = false,
                        message = "Inventory item not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = item,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetInventoryItem: {ex.Message}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPut("inventory/{productId}")]
        public async Task<ActionResult> UpdateInventory(int productId, [FromBody] UpdateInventoryRequest request)
        {
            try
            {
                _logger.LogInformation($"🔄 UpdateInventory called: ProductId={productId}, Price={request.Price}, Stock={request.Stock}");

                var item = await _context.DistributorInventories
                    .FirstOrDefaultAsync(di => di.ProductId == productId && di.DistributorId == request.DistributorId);

                if (item == null)
                {
                    return NotFound(new { 
                        success = false,
                        message = "Inventory item not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                item.Price = request.Price;
                item.Stock = request.Stock;
                item.DeliveryDays = request.DeliveryDays;
                item.IsActive = request.IsActive;
                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Inventory updated successfully for product {productId}");

                return Ok(new { 
                    success = true,
                    message = "Inventory updated successfully", 
                    data = new {
                        productId = productId,
                        price = item.Price,
                        stock = item.Stock,
                        deliveryDays = item.DeliveryDays,
                        isActive = item.IsActive
                    },
                    updatedAt = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in UpdateInventory: {ex.Message}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpDelete("inventory/{productId}")]
        public async Task<ActionResult> RemoveFromInventory(int productId, [FromQuery] int distributorId = 1)
        {
            try
            {
                _logger.LogInformation($"🗑️ RemoveFromInventory called: ProductId={productId}, DistributorId={distributorId}");

                var item = await _context.DistributorInventories
                    .FirstOrDefaultAsync(di => di.ProductId == productId && di.DistributorId == distributorId);

                if (item == null)
                {
                    return NotFound(new { 
                        success = false,
                        message = "Inventory item not found",
                        timestamp = DateTime.UtcNow
                    });
                }

                _context.DistributorInventories.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Item removed from inventory successfully: ProductId={productId}");

                return Ok(new { 
                    success = true,
                    message = "Item removed from inventory successfully", 
                    removedAt = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RemoveFromInventory: {ex.Message}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get distributor dashboard statistics
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<ActionResult> GetDashboardStats([FromQuery] int distributorId = 1)
        {
            try
            {
                var stats = new
                {
                    PendingQuotations = await _context.QuotationRequests
                        .Where(qr => qr.Status == "Pending")
                        .CountAsync(),
                    TotalOrders = await _context.Orders
                        .Where(o => o.DistributorId == distributorId)
                        .CountAsync(),
                    MonthlyRevenue = await _context.Orders
                        .Where(o => o.DistributorId == distributorId && 
                                   o.OrderDate >= DateTime.UtcNow.AddDays(-30))
                        .SumAsync(o => o.TotalAmount),
                    ProductsInInventory = await _context.DistributorInventories
                        .Where(di => di.DistributorId == distributorId && di.IsActive)
                        .CountAsync(),
                    LowStockItems = await _context.DistributorInventories
                        .Where(di => di.DistributorId == distributorId && di.Stock > 0 && di.Stock <= 10)
                        .CountAsync(),
                    OutOfStockItems = await _context.DistributorInventories
                        .Where(di => di.DistributorId == distributorId && di.Stock == 0)
                        .CountAsync()
                };

                return Ok(new
                {
                    success = true,
                    data = stats,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetDashboardStats: {ex.Message}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Initialize inventory for all products (useful for new distributors)
        /// </summary>
        [HttpPost("inventory/initialize")]
        public async Task<ActionResult> InitializeInventory([FromQuery] int distributorId = 1)
        {
            try
            {
                _logger.LogInformation($"🚀 InitializeInventory called for distributorId={distributorId}");

                // Get all products that don't have inventory for this distributor
                var productsWithoutInventory = await _context.Products
                    .Where(p => p.IsActive && 
                               !_context.DistributorInventories
                                   .Any(di => di.DistributorId == distributorId && di.ProductId == p.Id))
                    .ToListAsync();

                if (!productsWithoutInventory.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "All products already have inventory entries",
                        initializedCount = 0,
                        timestamp = DateTime.UtcNow
                    });
                }

                var newInventoryItems = new List<DistributorInventory>();
                var random = new Random();

                foreach (var product in productsWithoutInventory)
                {
                    var basePrice = GetEstimatedPrice(product.Name);
                    var priceVariation = 1 + (random.NextSingle() * 0.2f - 0.1f); // ±10% variation
                    
                    newInventoryItems.Add(new DistributorInventory
                    {
                        DistributorId = distributorId,
                        ProductId = product.Id,
                        Price = Math.Round(basePrice * (decimal)priceVariation, 2),
                        Stock = random.Next(20, 150),
                        DeliveryDays = random.Next(1, 14),
                        IsActive = true,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                _context.DistributorInventories.AddRange(newInventoryItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Initialized {newInventoryItems.Count} inventory items for distributor {distributorId}");

                return Ok(new
                {
                    success = true,
                    message = $"Successfully initialized inventory for {newInventoryItems.Count} products",
                    initializedCount = newInventoryItems.Count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in InitializeInventory: {ex.Message}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        private decimal GetEstimatedPrice(string productName)
        {
            return productName.ToLower() switch
            {
                var name when name.Contains("iphone") => 999.00m,
                var name when name.Contains("samsung") && name.Contains("galaxy") => 899.00m,
                var name when name.Contains("ipad") => 799.00m,
                var name when name.Contains("macbook") => 1999.00m,
                var name when name.Contains("airpods") => 249.00m,
                var name when name.Contains("laptop") => 1299.00m,
                var name when name.Contains("tablet") => 599.00m,
                var name when name.Contains("phone") => 699.00m,
                _ => 399.00m
            };
        }
    }

    public class UpdateInventoryRequest
    {
        public int DistributorId { get; set; } = 1;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsActive { get; set; } = true;
    }
}