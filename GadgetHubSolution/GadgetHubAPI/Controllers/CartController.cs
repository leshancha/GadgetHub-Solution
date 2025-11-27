using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Data;
using GadgetHubAPI.Models;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetCart([FromQuery] int customerId = 1)
        {
            try
            {
                _logger.LogInformation($"?? API GetCart called for customerId={customerId}");
                _logger.LogInformation($"?? Database context hash: {_context.GetHashCode()}");

                // ? ENHANCED: Check database connection and customer count
                try
                {
                    var totalCustomers = await _context.Customers.CountAsync();
                    _logger.LogInformation($"?? Total customers in database: {totalCustomers}");
                    
                    if (totalCustomers == 0)
                    {
                        _logger.LogError($"? No customers found in database! Database seeding may have failed.");
                        return BadRequest(new { 
                            message = "Database not properly seeded - no customers found",
                            suggestion = "Run the database reset script",
                            timestamp = DateTime.UtcNow
                        });
                    }
                    
                    // List first few customers for debugging
                    var customers = await _context.Customers.Take(5).Select(c => new { c.Id, c.Name, c.Email, c.IsActive }).ToListAsync();
                    _logger.LogInformation($"?? Available customers: {string.Join(", ", customers.Select(c => $"ID={c.Id} Name={c.Name} Email={c.Email} Active={c.IsActive}"))}");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError($"? Database connection error: {dbEx.Message}");
                    return StatusCode(500, new { 
                        message = "Database connection failed",
                        error = dbEx.Message,
                        timestamp = DateTime.UtcNow
                    });
                }

                // Check if the specific customer exists
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    _logger.LogError($"? Customer {customerId} not found in database");
                    
                    // ? ENHANCED: Return helpful error with suggestions
                    return BadRequest(new { 
                        message = $"Customer with ID {customerId} not found",
                        suggestion = "Try using customer ID 1 or run the database reset script",
                        availableCustomers = await _context.Customers.Take(3).Select(c => new { c.Id, c.Name, c.Email }).ToListAsync(),
                        timestamp = DateTime.UtcNow
                    });
                }
                _logger.LogInformation($"? Customer found: {customer.Name} ({customer.Email}) - Active: {customer.IsActive}");

                // ? ENHANCED: Check total cart items in database
                var totalCartItems = await _context.CartItems.CountAsync();
                var customerCartItems = await _context.CartItems.CountAsync(ci => ci.CustomerId == customerId);
                _logger.LogInformation($"?? Cart items - Total: {totalCartItems}, For customer {customerId}: {customerCartItems}");

                // Get cart items for this specific customer
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .ThenInclude(p => p.Category)
                    .Where(ci => ci.CustomerId == customerId)
                    .OrderByDescending(ci => ci.DateAdded)
                    .ToListAsync();

                _logger.LogInformation($"?? Found {cartItems.Count} cart items for customer {customerId}");

                if (cartItems.Any())
                {
                    foreach (var item in cartItems)
                    {
                        _logger.LogInformation($"?? Cart item: ID={item.Id}, ProductId={item.ProductId}, ProductName={item.Product?.Name ?? "Unknown"}, Quantity={item.Quantity}, DateAdded={item.DateAdded}");
                    }
                }
                else
                {
                    _logger.LogInformation($"?? Cart is empty for customer {customerId}");
                }

                var cartItemsWithPricing = new List<object>();

                foreach (var cartItem in cartItems)
                {
                    var lowestPrice = await _context.DistributorInventories
                        .Where(di => di.ProductId == cartItem.ProductId && di.IsActive)
                        .OrderBy(di => di.Price)
                        .Select(di => di.Price)
                        .FirstOrDefaultAsync();

                    var estimatedPrice = lowestPrice > 0 ? lowestPrice : 299.99m;
                    var totalPrice = estimatedPrice * cartItem.Quantity;

                    cartItemsWithPricing.Add(new
                    {
                        Id = cartItem.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        ProductBrand = cartItem.Product.Brand,
                        ProductImage = cartItem.Product.ImageUrl,
                        Category = cartItem.Product.Category.Name,
                        Quantity = cartItem.Quantity,
                        Price = estimatedPrice,
                        TotalPrice = totalPrice,
                        EstimatedPrice = estimatedPrice,
                        DateAdded = cartItem.DateAdded
                    });
                }

                var total = cartItemsWithPricing.Sum(ci => (decimal)((dynamic)ci).TotalPrice);

                var result = new
                {
                    Items = cartItemsWithPricing,
                    TotalItems = cartItems.Sum(ci => ci.Quantity),
                    EstimatedTotal = total,
                    RetrievedAt = DateTime.UtcNow,
                    Debug = new
                    {
                        CustomerId = customerId,
                        CustomerExists = customer != null,
                        CustomerName = customer?.Name,
                        CartItemsInDatabase = cartItems.Count,
                        TotalCartItemsAllCustomers = totalCartItems,
                        DatabaseContextHash = _context.GetHashCode()
                    }
                };

                _logger.LogInformation($"? Returning cart with {cartItemsWithPricing.Count} items, total: ${total}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error in GetCart: {ex.Message}");
                _logger.LogError($"? Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                _logger.LogInformation($"?? API AddToCart called: CustomerId={request.CustomerId}, ProductId={request.ProductId}, Quantity={request.Quantity}");
                _logger.LogInformation($"?? Database context hash: {_context.GetHashCode()}");
                _logger.LogInformation($"?? Connection string: {_context.Database.GetConnectionString()}");

                // Check if product exists
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    _logger.LogError($"? Product {request.ProductId} not found in database");
                    return BadRequest(new { message = "Product not found" });
                }
                _logger.LogInformation($"? Product found: {product.Name}");

                // Check if customer exists
                var customer = await _context.Customers.FindAsync(request.CustomerId);
                if (customer == null)
                {
                    _logger.LogError($"? Customer {request.CustomerId} not found in database");
                    return BadRequest(new { message = "Customer not found" });
                }
                _logger.LogInformation($"? Customer found: {customer.Name}");

                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CustomerId == request.CustomerId && ci.ProductId == request.ProductId);

                _logger.LogInformation($"?? Existing cart item found: {existingItem != null}");

                if (existingItem != null)
                {
                    var oldQuantity = existingItem.Quantity;
                    existingItem.Quantity += request.Quantity;
                    _logger.LogInformation($"?? Updated existing item quantity from {oldQuantity} to {existingItem.Quantity}");
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CustomerId = request.CustomerId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        DateAdded = DateTime.UtcNow
                    };

                    _context.CartItems.Add(cartItem);
                    _logger.LogInformation($"? Created new cart item: CustomerId={cartItem.CustomerId}, ProductId={cartItem.ProductId}, Quantity={cartItem.Quantity}");
                }

                var saveResult = await _context.SaveChangesAsync();
                _logger.LogInformation($"?? SaveChanges result: {saveResult} rows affected");

                // ? ENHANCED: Immediately verify the item was saved
                var verificationQuery = await _context.CartItems
                    .Where(ci => ci.CustomerId == request.CustomerId)
                    .ToListAsync();
                _logger.LogInformation($"?? Verification query found {verificationQuery.Count} items for customer {request.CustomerId}");

                if (verificationQuery.Any())
                {
                    foreach (var item in verificationQuery)
                    {
                        _logger.LogInformation($"?? Verified item: ID={item.Id}, ProductId={item.ProductId}, Quantity={item.Quantity}");
                    }
                }
                else
                {
                    _logger.LogError($"? CRITICAL: No items found in verification query after SaveChanges!");
                }

                return Ok(new { 
                    message = "Product added to cart successfully", 
                    addedAt = DateTime.UtcNow,
                    customerId = request.CustomerId,
                    productId = request.ProductId,
                    quantity = request.Quantity,
                    saveResult = saveResult,
                    verifiedItemCount = verificationQuery.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error in AddToCart: {ex.Message}");
                _logger.LogError($"? Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("update-by-product")]
        public async Task<ActionResult> UpdateCartItemByProduct([FromBody] UpdateCartByProductRequest request)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CustomerId == request.CustomerId && ci.ProductId == request.ProductId);

                if (cartItem == null)
                {
                    return NotFound(new { message = "Cart item not found" });
                }

                if (request.Quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = request.Quantity;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cart item updated successfully", updatedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error in UpdateCartItemByProduct: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("remove-by-product")]
        public async Task<ActionResult> RemoveFromCartByProduct([FromBody] RemoveCartByProductRequest request)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CustomerId == request.CustomerId && ci.ProductId == request.ProductId);

                if (cartItem == null)
                {
                    return NotFound(new { message = "Cart item not found" });
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Item removed from cart successfully", removedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error in RemoveFromCartByProduct: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("database-diagnostic")]
        public async Task<ActionResult> DatabaseDiagnostic()
        {
            try
            {
                var diagnostic = new
                {
                    DatabaseConnection = "Connected",
                    Timestamp = DateTime.UtcNow,
                    Tables = new
                    {
                        Customers = await _context.Customers.CountAsync(),
                        Distributors = await _context.Distributors.CountAsync(),
                        Admins = await _context.Admins.CountAsync(),
                        Products = await _context.Products.CountAsync(),
                        Categories = await _context.Categories.CountAsync(),
                        CartItems = await _context.CartItems.CountAsync(),
                        DistributorInventories = await _context.DistributorInventories.CountAsync()
                    },
                    SampleData = new
                    {
                        FirstCustomer = await _context.Customers.FirstOrDefaultAsync(),
                        FirstProduct = await _context.Products.FirstOrDefaultAsync(),
                        RecentCartItems = await _context.CartItems
                            .Include(ci => ci.Product)
                            .OrderByDescending(ci => ci.DateAdded)
                            .Take(3)
                            .Select(ci => new { 
                                ci.Id, 
                                ci.CustomerId, 
                                ci.ProductId,
                                ProductName = ci.Product != null ? ci.Product.Name : "Unknown",
                                ci.Quantity, 
                                ci.DateAdded 
                            })
                            .ToListAsync()
                    }
                };

                _logger.LogInformation($"?? Database diagnostic completed successfully");
                return Ok(diagnostic);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Database diagnostic failed: {ex.Message}");
                return StatusCode(500, new { 
                    error = "Database diagnostic failed", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("update-product-images")]
        public async Task<ActionResult> UpdateProductImages()
        {
            try
            {
                _logger.LogInformation($"??? Product image update requested at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                
                // Call the enhanced ProductsController method for comprehensive fix
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(
                    "http://localhost:5079/api/products/create-physical-images", 
                    new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                );
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    return Ok(new { 
                        message = "Product images updated comprehensively!",
                        success = true,
                        timestamp = DateTime.UtcNow,
                        details = result
                    });
                }
                else
                {
                    // Fallback to database-only update
                    var products = await _context.Products.ToListAsync();
                    int updatedCount = 0;

                    var imageUpdates = new Dictionary<string, string>
                    {
                        // Phones
                        ["Apple iPhone 15 Pro"] = "images/Apple15.png",
                        ["Samsung Galaxy S24 Ultra"] = "images/s24ultra.png",
                        ["Google Pixel 8 Pro"] = "images/pixel.png",
                        ["OnePlus 12"] = "images/oneplus.png",
                        
                        // Tablets
                        ["Apple iPad Pro 12.9? (M2)"] = "images/ipad.png",
                        ["Samsung Galaxy Tab S9 Ultra"] = "images/s9tab.png",
                        ["Amazon Kindle Paperwhite"] = "images/paperwrit.png",
                        ["Lenovo Tab M10 Plus"] = "images/lenovotab.png",
                        
                        // Laptops
                        ["Apple MacBook Air 15? M2"] = "images/macbook.png",
                        ["Dell XPS 13 Plus"] = "images/dell.png",
                        ["ASUS ROG Zephyrus G16"] = "images/rog.png",
                        
                        // Accessories
                        ["Sony WH-1000XM5"] = "images/headsony.png",
                        ["Apple AirPods Pro (2nd Gen)"] = "images/airpod.png",
                        ["Logitech MX Master 3S"] = "images/mouse.png",
                        ["Samsung T7 Shield SSD 1TB"] = "images/ssd.png",
                        ["Apple Watch Series 9"] = "images/watch.png",
                        ["JBL Flip 6 Bluetooth Speaker"] = "images/jbl.png",
                        ["Fitbit Charge 6"] = "images/fitneswatch.png",
                        ["Bose QuietComfort Ultra"] = "images/bose.png",
                        
                        // Gadgets
                        ["GoPro HERO12 Black"] = "images/gopro.png",
                        ["DJI Mini 4 Pro"] = "images/Drone.png",
                        ["Oculus Quest 3"] = "images/vr.png",
                        ["Google Nest Hub (2nd Gen)"] = "images/nesthub.png"
                    };

                    foreach (var product in products)
                    {
                        if (imageUpdates.ContainsKey(product.Name))
                        {
                            var oldImageUrl = product.ImageUrl;
                            product.ImageUrl = imageUpdates[product.Name];
                            product.UpdatedAt = DateTime.UtcNow;
                            updatedCount++;
                            _logger.LogInformation($"? Updated {product.Name}: {oldImageUrl} ? {product.ImageUrl}");
                        }
                    }

                    await _context.SaveChangesAsync();
                    
                    return Ok(new { 
                        message = "Product image paths updated (fallback mode)!",
                        updatedCount = updatedCount,
                        totalProducts = products.Count,
                        timestamp = DateTime.UtcNow,
                        success = true,
                        note = "Database updated, but physical files may need manual creation"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error updating product images: {ex.Message}");
                return StatusCode(500, new { 
                    error = "Product image update failed", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }

    public class AddToCartRequest
    {
        public int CustomerId { get; set; } = 1;
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartByProductRequest
    {
        public int CustomerId { get; set; } = 1;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveCartByProductRequest
    {
        public int CustomerId { get; set; } = 1;
        public int ProductId { get; set; }
    }
}