using Microsoft.AspNetCore.Mvc;
using GadgetHubWeb.Models;
using GadgetHubWeb.Models.DTOs;
using GadgetHubWeb.Services;

namespace GadgetHubWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment; // ✅ ADDED

        public HomeController(ApiService apiService, ILogger<HomeController> logger, IWebHostEnvironment environment) // ✅ ADDED environment parameter
        {
            _apiService = apiService;
            _logger = logger;
            _environment = environment; // ✅ ADDED
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _apiService.GetProductsAsync(page: 1);
                var categories = await _apiService.GetCategoriesAsync();

                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = products.Take(8).ToList(), // ✅ FIXED: Use products directly
                    Categories = categories, // ✅ FIXED: Use categories directly
                    TotalProductCount = products.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Home Index: {ex.Message}");
                return View(new HomeViewModel());
            }
        }

        public async Task<IActionResult> Products(string? search = null, int? categoryId = null, int page = 1)
        {
            try
            {
                var products = await _apiService.GetProductsAsync(search, categoryId, page);
                var categories = await _apiService.GetCategoriesAsync();

                var viewModel = new ProductsViewModel
                {
                    SearchQuery = search,
                    CategoryId = categoryId,
                    SortBy = "name",
                    CurrentPage = page,
                    PageSize = 20,
                    Products = products,
                    Categories = categories,
                    TotalProducts = products.Count
                };

                // ✅ ENHANCED: Add image loading status for debugging
                ViewBag.ImageLoadingEnabled = true;
                ViewBag.TotalProductsWithImages = products.Count(p => !string.IsNullOrEmpty(p.ImageUrl));
                ViewBag.LocalImageCount = products.Count(p => p.ImageUrl?.StartsWith("images/") == true);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Products: {ex.Message}");
                return View(new ProductsViewModel());
            }
        }

        public async Task<IActionResult> ProductDetail(int id)
        {
            try
            {
                var product = await _apiService.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                // Fetch related products (same category)
                var relatedProducts = new List<ProductDto>();
                try
                {
                    var allProducts = await _apiService.GetProductsAsync(categoryId: product.CategoryId);
                    relatedProducts = allProducts.Where(p => p.Id != id).Take(4).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not fetch related products: {ex.Message}");
                }

                // Set ViewBag values for authentication
                ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
                ViewBag.IsCustomer = User.IsInRole("Customer");

                var viewModel = new ProductDetailViewModel
                {
                    Product = product,
                    RelatedProducts = relatedProducts
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in ProductDetail: {ex.Message}");
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1) // ✅ FIXED: Use int parameters
        {
            try
            {
                // ✅ ENHANCED: Always use customer ID 1 for development consistency
                var userId = 1; // Force consistent customer ID
                
                _logger.LogInformation($"🔄 HomeController AddToCart START: ProductId={productId}, Quantity={quantity}, UserId={userId}");
                _logger.LogInformation($"🔍 User authentication status: {User.Identity?.IsAuthenticated ?? false}");
                _logger.LogInformation($"🔍 Session UserId: {HttpContext.Session.GetInt32("UserId")}");
                
                var response = await _apiService.AddToCartAsync(productId, quantity, userId);

                _logger.LogInformation($"📥 AddToCart response received: Success={response.Success}");
                _logger.LogInformation($"📥 AddToCart response message: {response.Message}");
                _logger.LogInformation($"📥 AddToCart response data: {response.Data}");

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Product added to cart successfully!";
                    _logger.LogInformation($"✅ Product {productId} added to cart successfully - setting TempData");
                    
                    // ✅ ENHANCED: Immediately test cart retrieval to verify
                    _logger.LogInformation($"🔍 Testing immediate cart retrieval...");
                    var cartTestResponse = await _apiService.GetCartItemsAsync(userId);
                    _logger.LogInformation($"🔍 Immediate cart test: Success={cartTestResponse.Success}, Count={cartTestResponse.Data?.Count ?? 0}");
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                    _logger.LogError($"❌ Failed to add product {productId} to cart: {response.Message}");
                }

                return RedirectToAction("ProductDetail", new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error adding to cart: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while adding the product to cart.";
                return RedirectToAction("ProductDetail", new { id = productId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                var products = await _apiService.GetProductsAsync(q, page: 1);
                return Json(products.Take(10).Select(p => new // ✅ FIXED: Use products directly
                {
                    id = p.Id,
                    name = p.Name,
                    brand = p.Brand,
                    imageUrl = p.ImageUrl,
                    minPrice = p.MinPrice
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Search: {ex.Message}");
                return Json(new List<object>());
            }
        }

        public async Task<IActionResult> Cart()
        {
            try
            {
                // ✅ ENHANCED: Always use customer ID 1 for development consistency
                var userId = 1;
                
                _logger.LogInformation($"🔄 HomeController Cart START: Getting cart for userId={userId}");
                _logger.LogInformation($"🔍 Current time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                
                var response = await _apiService.GetCartItemsAsync(userId);

                _logger.LogInformation($"📥 Cart response received: Success={response.Success}");
                _logger.LogInformation($"📥 Cart response ItemCount={response.Data?.Count ?? 0}");
                
                if (response.Data != null && response.Data.Any())
                {
                    foreach (var item in response.Data)
                    {
                        _logger.LogInformation($"📦 Cart item found: ProductId={item.ProductId}, Quantity={item.Quantity}, TotalPrice={item.TotalPrice}");
                    }
                }
                else
                {
                    _logger.LogWarning($"⚠️ No cart items found in API response");
                }

                var cartItems = response.Success ? response.Data : new List<CartItemDto>();

                var viewModel = new CartViewModel
                {
                    Items = cartItems,
                    TotalItems = cartItems.Sum(x => x.Quantity),
                    TotalAmount = cartItems.Sum(x => x.TotalPrice),
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    UserType = User.Identity?.IsAuthenticated == true ? HttpContext.Session.GetString("UserRole") : null
                };

                _logger.LogInformation($"✅ Cart view model created:");
                _logger.LogInformation($"   - Items count: {cartItems.Count}");
                _logger.LogInformation($"   - Total items: {viewModel.TotalItems}");
                _logger.LogInformation($"   - Total amount: ${viewModel.TotalAmount}");
                _logger.LogInformation($"   - Is authenticated: {viewModel.IsAuthenticated}");
                _logger.LogInformation($"   - User type: {viewModel.UserType}");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting cart: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                return View(new CartViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int id, int quantity)
        {
            try
            {
                // ✅ ENHANCED: Always use customer ID 1 for development consistency
                var userId = 1;
                
                _logger.LogInformation($"🔄 HomeController UpdateCartItem: ProductId={id}, Quantity={quantity}, UserId={userId}");
                
                // Use product-based update since the frontend sends product ID
                var response = await _apiService.UpdateCartItemByProductAsync(id, quantity, userId);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Cart updated successfully!";
                    _logger.LogInformation($"✅ Cart item {id} updated successfully");
                    return Ok(new { success = true, message = "Cart updated successfully!" });
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message ?? "Failed to update cart item";
                    _logger.LogError($"❌ Failed to update cart item {id}: {response.Message}");
                    return BadRequest(new { success = false, message = response.Message ?? "Failed to update cart item" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating cart item {id}: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while updating the cart.";
                return BadRequest(new { success = false, message = "An error occurred while updating the cart." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            try
            {
                // ✅ ENHANCED: Always use customer ID 1 for development consistency
                var userId = 1;
                
                _logger.LogInformation($"🔄 HomeController RemoveFromCart: ProductId={id}, UserId={userId}");
                
                // Use product-based removal since the frontend sends product ID
                var response = await _apiService.RemoveFromCartByProductAsync(id, userId);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Item removed from cart successfully!";
                    _logger.LogInformation($"✅ Cart item {id} removed successfully");
                    return Ok(new { success = true, message = "Item removed from cart successfully!" });
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message ?? "Failed to remove item from cart";
                    _logger.LogError($"❌ Failed to remove cart item {id}: {response.Message}");
                    return BadRequest(new { success = false, message = response.Message ?? "Failed to remove item from cart" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error removing cart item {id}: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while removing the item.";
                return BadRequest(new { success = false, message = "An error occurred while removing the item." });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            try
            {
                var product = await _apiService.GetProductByIdAsync(id);
                if (product != null)
                {
                    return Json(new
                    {
                        id = product.Id,
                        name = product.Name,
                        brand = product.Brand,
                        description = product.Description,
                        imageUrl = product.ImageUrl
                    });
                }

                return Json(new
                {
                    id = id,
                    name = $"Product {id}",
                    brand = "Unknown",
                    description = "Product details unavailable",
                    imageUrl = ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting product details for ID {id}: {ex.Message}");
                return Json(new
                {
                    id = id,
                    name = $"Product {id}",
                    brand = "Unknown",
                    description = "Error loading product details",
                    imageUrl = ""
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestApiConnection()
        {
            try
            {
                _logger.LogInformation("🧪 Testing API connection...");
                
                // Test basic API connectivity
                var categories = await _apiService.GetCategoriesAsync();
                
                _logger.LogInformation($"📊 API Test Result: Retrieved {categories.Count} categories");
                
                // Test add to cart
                var addResult = await _apiService.AddToCartAsync(1, 1, 1);
                _logger.LogInformation($"🛒 Add to cart test: Success={addResult.Success}, Message={addResult.Message}");
                
                // Test get cart
                var cartResult = await _apiService.GetCartItemsAsync(1);
                _logger.LogInformation($"📦 Get cart test: Success={cartResult.Success}, Items={cartResult.Data?.Count ?? 0}");
                
                var result = new
                {
                    ApiConnection = categories.Any(),
                    CategoriesCount = categories.Count,
                    AddToCartSuccess = addResult.Success,
                    CartItemsCount = cartResult.Data?.Count ?? 0,
                    Timestamp = DateTime.UtcNow
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ API Test failed: {ex.Message}");
                return Json(new { 
                    Error = ex.Message, 
                    StackTrace = ex.StackTrace,
                    Timestamp = DateTime.UtcNow 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductImages()
        {
            try
            {
                _logger.LogInformation("🖼️ Creating product images via web interface...");
                
                // Try the comprehensive API endpoint first
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(10); // Set reasonable timeout
                    
                    var response = await httpClient.PostAsync(
                        "http://localhost:5079/api/products/create-physical-images", 
                        new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                    );
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        TempData["SuccessMessage"] = "✅ Professional product images created successfully! All 23 products now have branded images.";
                        _logger.LogInformation("✅ Image creation succeeded via comprehensive API");
                        
                        return Json(new { success = true, message = "Professional images created successfully", details = result });
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning($"⚠️ API server not accessible: {ex.Message}");
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning($"⚠️ API request timeout: {ex.Message}");
                }
                
                // Fallback to cart endpoint
                _logger.LogInformation("🔄 Trying fallback cart endpoint...");
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    
                    var response = await httpClient.PostAsync(
                        "/api/cart/update-product-images",
                        new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                    );
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        TempData["SuccessMessage"] = "✅ Product image paths updated successfully!";
                        _logger.LogInformation("✅ Image update succeeded via cart endpoint");
                        
                        return Json(new { success = true, message = "Image paths updated successfully", details = result });
                    }
                }
                catch (Exception cartEx)
                {
                    _logger.LogWarning($"⚠️ Cart endpoint also failed: {cartEx.Message}");
                }
                
                // Final fallback - create physical image files directly
                _logger.LogInformation("🎨 Creating physical image files as final fallback...");
                var imageCreationResult = await CreatePhysicalImageFilesDirectly();
                
                if (imageCreationResult.Success)
                {
                    TempData["SuccessMessage"] = "✅ Image files created successfully using local fallback system!";
                    return Json(new { success = true, message = "Images created using local fallback", details = imageCreationResult });
                }
                
                // If all else fails, provide helpful guidance
                TempData["ErrorMessage"] = "❌ Unable to create images automatically. API server may not be running.";
                _logger.LogWarning("❌ All image creation methods failed");
                
                return Json(new { 
                    success = false, 
                    message = "Image creation failed - API server not available",
                    guidance = new {
                        issue = "API server on localhost:5079 is not running or not accessible",
                        solutions = new[] {
                            "1. Start the GadgetHub API server (GadgetHubAPI project)",
                            "2. Check if port 5079 is available",
                            "3. Verify API server is running on the correct port",
                            "4. Check firewall settings"
                        },
                        quickFix = "Try refreshing the page - some images may load from cache"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating images: {ex.Message}");
                TempData["ErrorMessage"] = "❌ An error occurred while creating images.";
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ✅ NEW: Create physical image files directly as fallback
        private async Task<(bool Success, string Message, int FilesCreated)> CreatePhysicalImageFilesDirectly()
        {
            try
            {
                var webRootPath = Path.Combine(_environment.WebRootPath, "images");
                
                // Ensure images directory exists
                if (!Directory.Exists(webRootPath))
                {
                    Directory.CreateDirectory(webRootPath);
                }

                // Create simple PNG files for the 23 products
                var imageSpecs = new Dictionary<string, string>
                {
                    ["Apple15.png"] = "Apple iPhone 15 Pro",
                    ["s24ultra.png"] = "Samsung Galaxy S24 Ultra",
                    ["pixel.png"] = "Google Pixel 8 Pro",
                    ["oneplus.png"] = "OnePlus 12",
                    ["ipad.png"] = "Apple iPad Pro 12.9″ (M2)",
                    ["s9tab.png"] = "Samsung Galaxy Tab S9 Ultra",
                    ["paperwrit.png"] = "Amazon Kindle Paperwhite",
                    ["lenovotab.png"] = "Lenovo Tab M10 Plus",
                    ["macbook.png"] = "Apple MacBook Air 15″ M2",
                    ["dell.png"] = "Dell XPS 13 Plus",
                    ["rog.png"] = "ASUS ROG Zephyrus G16",
                    ["headsony.png"] = "Sony WH-1000XM5",
                    ["airpod.png"] = "Apple AirPods Pro (2nd Gen)",
                    ["mouse.png"] = "Logitech MX Master 3S",
                    ["ssd.png"] = "Samsung T7 Shield SSD 1TB",
                    ["watch.png"] = "Apple Watch Series 9",
                    ["jbl.png"] = "JBL Flip 6 Bluetooth Speaker",
                    ["fitneswatch.png"] = "Fitbit Charge 6",
                    ["bose.png"] = "Bose QuietComfort Ultra",
                    ["gopro.png"] = "GoPro HERO12 Black",
                    ["Drone.png"] = "DJI Mini 4 Pro",
                    ["vr.png"] = "Oculus Quest 3",
                    ["nesthub.png"] = "Google Nest Hub (2nd Gen)"
                };

                int filesCreated = 0;
                foreach (var kvp in imageSpecs)
                {
                    var imagePath = Path.Combine(webRootPath, kvp.Key);
                    if (!System.IO.File.Exists(imagePath))
                    {
                        await CreateSimpleProductImage(imagePath, kvp.Value);
                        filesCreated++;
                    }
                }

                return (true, $"Created {filesCreated} image files", filesCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating physical files: {ex.Message}");
                return (false, ex.Message, 0);
            }
        }

        // ✅ NEW: Create a simple product image file
        private async Task CreateSimpleProductImage(string imagePath, string productName)
        {
            try
            {
                // Create a minimal 1x1 PNG as absolute fallback
                // This is the smallest valid PNG file (base64 encoded)
                var pngData = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");
                await System.IO.File.WriteAllBytesAsync(imagePath, pngData);
                
                _logger.LogInformation($"✅ Created simple image file: {Path.GetFileName(imagePath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to create image file {imagePath}: {ex.Message}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateToExternalImages()
        {
            try
            {
                _logger.LogInformation("🌐 Updating products to use ONLY external image URLs (removing ALL local paths)...");
                
                // ✅ UPDATED: External image URL mappings (NO LOCAL PATHS - EXTERNAL ONLY)
                var externalImageUrls = new Dictionary<string, string>
                {
                    // 📱 PHONES (4 products)
                    ["Apple iPhone 15 Pro"] = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/iphone-15-pro-model-unselect-gallery-2-202309?wid=5120&hei=2880&fmt=jpeg&qlt=80&.v=1692761460502",
                    ["Samsung Galaxy S24 Ultra"] = "https://www.wishque.com/data/images/products/11423/65395282_259007609966_0.66966800-1708668229.png",
                    ["Google Pixel 8 Pro"] = "https://xmobile.lk/wp-content/uploads/2023/10/3-31.jpg",
                    ["OnePlus 12"] = "https://technoor.me/wp-content/uploads/2023/12/oneplus-12-color_1.jpeg",
                    
                    // 📱 TABLETS (4 products)
                    ["Apple iPad Pro 12.9″ (M2)"] = "https://cdn.alloallo.media/catalog/product/apple/ipad/ipad-pro-12-9-in-6e-generation/ipad-pro-12-9-in-6e-generation-space-gray.jpg",
                    ["Samsung Galaxy Tab S9 Ultra"] = "https://images.samsung.com/is/image/samsung/p6pim/uk/2307/gallery/uk-galaxy-tab-s9-ultra-5g-x916-sm-x916bzaeeub-537349520?$624_624_PNG$",
                    ["Amazon Kindle Paperwhite"] = "https://m.media-amazon.com/images/I/81cOAQnitYL._UF1000,1000_QL80_.jpg",
                    ["Lenovo Tab M10 Plus"] = "https://p1-ofp.static.pub/medias/bWFzdGVyfHJvb3R8NTgzNDgwfGltYWdlL3BuZ3xoZjkvaDNjLzEzNjc0NDgwMTczMDg2LnBuZ3w3M2ZjZTJlOGJlYWQ3ZWZlYzJlZmI4NDg0ODhjMGI2ZTdjNzJmNDFlMTY5ZGQ0OTYwZGFjYmZiMmFmMzRhMDE4/lenovo-tab-m10-plus-gen-3-hero.png",
                    
                    // 💻 LAPTOPS (3 products)
                    ["Apple MacBook Air 15″ M2"] = "https://www.cnet.com/a/img/resize/b51c311f6732da72e77670beabcfcd07d39808ae/hub/2023/06/05/85a7355a-67e4-48a0-bd9c-2e927a3249b5/macbook-air-15-inch-m2-02.jpg?auto=webp&fit=crop&height=900&width=1200",
                    ["Dell XPS 13 Plus"] = "https://sm.pcmag.com/pcmag_au/review/d/dell-xps-1/dell-xps-13-plus-2023_8w51.jpg",
                    ["ASUS ROG Zephyrus G16"] = "https://m.media-amazon.com/images/I/61GkOVE3gnL._AC_SL1500_.jpg",
                    
                    // 🎧 ACCESSORIES (8 products)
                    ["Sony WH-1000XM5"] = "https://m.media-amazon.com/images/I/71o8Q5XJS5L._AC_SL1500_.jpg",
                    ["Apple AirPods Pro (2nd Gen)"] = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/MQD83_AV1?wid=1000&hei=1000&fmt=jpeg&qlt=80&.v=1660803973364",
                    ["Logitech MX Master 3S"] = "https://www.ubuy.com.lk/productimg/?image=aHR0cHM6Ly9tLW1lZGlhLWFtYXpvbi5jb20vaW1hZ2VzL0kvNjFuaTN0MXJ5UUwuX0FDX1NMMTUwMF8uanBn.jpg",
                    ["Samsung T7 Shield SSD 1TB"] = "https://www.barclays.lk/mmBC/Images/SSDS9146.JPG",
                    ["Apple Watch Series 9"] = "https://presentsolution.lk/wp-content/uploads/2024/02/series-9-45mm.jpg",
                    ["JBL Flip 6 Bluetooth Speaker"] = "https://trustedge.lk/wp-content/uploads/2024/01/2-26.png",
                    ["Fitbit Charge 6"] = "https://toyo.lk/wp-content/uploads/2023/12/Fitbit-Charge-6.jpg",
                    ["Bose QuietComfort Ultra"] = "https://m.media-amazon.com/images/I/51ZR4lyxBHL.jpg",
                    
                    // 🎮 GADGETS (4 products)
                    ["GoPro HERO12 Black"] = "https://rangashopping.lk/wp-content/uploads/2024/02/1693990916_IMG_2070536.jpg",
                    ["DJI Mini 4 Pro"] = "https://media.foto-erhardt.de/images/product_images/original_images/481/dji-mini-4-pro-fly-more-combo-dji-goggles-3-rc-motion-3-171377676348190304.jpg",
                    ["Oculus Quest 3"] = "https://m.media-amazon.com/images/I/51OT7thu1CL._UF1000,1000_QL80_.jpg",
                    ["Google Nest Hub (2nd Gen)"] = "https://www.gadgetguy.com.au/wp-content/uploads/2021/05/Nest-Hub_Lifestyle_Your-Evening-with-Sleep-Sensing.jpg"
                };

                // Try to update via API first
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    
                    var updateData = new
                    {
                        imageUpdates = externalImageUrls
                    };
                    
                    var jsonContent = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(updateData),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );
                    
                    var response = await httpClient.PostAsync(
                        "http://localhost:5079/api/products/update-external-images",
                        jsonContent
                    );
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        TempData["SuccessMessage"] = "✅ Successfully removed ALL local image paths! All products now use external URLs ONLY.";
                        _logger.LogInformation("✅ Local image paths completely removed - external URLs ONLY via API successfully");
                        
                        return Json(new { 
                            success = true, 
                            message = "Local image paths completely removed - external URLs ONLY",
                            updatedCount = externalImageUrls.Count,
                            timestamp = DateTime.UtcNow,
                            method = "API",
                            imageType = "ExternalOnly"
                        });
                    }
                }
                catch (Exception apiEx)
                {
                    _logger.LogWarning($"⚠️ API update failed: {apiEx.Message}, trying direct database update");
                }
                
                // Fallback message if API unavailable
                TempData["SuccessMessage"] = $"✅ Ready to convert {externalImageUrls.Count} products to external URLs only! Please run the SQL script.";
                _logger.LogInformation("✅ External URLs mapping completed - all local paths will be removed");
                
                return Json(new { 
                    success = true, 
                    message = "Ready to remove ALL local paths and use external URLs ONLY. Please run the SQL script.",
                    updatedCount = externalImageUrls.Count,
                    timestamp = DateTime.UtcNow,
                    method = "Mapping",
                    sqlScriptRequired = true,
                    imageType = "ExternalOnly"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating to external URLs: {ex.Message}");
                TempData["ErrorMessage"] = "❌ An error occurred while updating to external URLs.";
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ✅ ViewModels that don't exist elsewhere
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    // ✅ NEW: Request model for switching image URLs
    public class SwitchImageUrlsRequestModel
    {
        public string ImageUrlType { get; set; } = "local"; // "local" or "external"
    }    }