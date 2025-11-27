using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Data;
using GadgetHubAPI.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // ? ENHANCED: Create physical image files and update database paths
        [HttpPost("create-physical-images")]
        public async Task<ActionResult> CreatePhysicalImages()
        {
            try
            {
                _logger.LogInformation("??? Starting comprehensive image creation and database update...");
                
                // Get the web root path correctly
                var webRootPath = Path.Combine(_environment.ContentRootPath, "..", "GadgetHubWeb", "wwwroot");
                var imagesPath = Path.Combine(webRootPath, "images");
                
                // Ensure images directory exists
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                    _logger.LogInformation($"? Created images directory: {imagesPath}");
                }

                var products = await _context.Products.ToListAsync();
                int createdFiles = 0;
                int updatedDatabase = 0;

                // EXACT 23 PRODUCTS IMAGE SPECIFICATIONS
                var imageSpecs = new Dictionary<string, (string filename, string brand, string category)>
                {
                    // Phones (4 products)
                    ["Apple iPhone 15 Pro"] = ("Apple15.png", "Apple", "Phone"),
                    ["Samsung Galaxy S24 Ultra"] = ("s24ultra.png", "Samsung", "Phone"),
                    ["Google Pixel 8 Pro"] = ("pixel.png", "Google", "Phone"),
                    ["OnePlus 12"] = ("oneplus.png", "OnePlus", "Phone"),
                    
                    // Tablets (4 products)
                    ["Apple iPad Pro 12.9? (M2)"] = ("ipad.png", "Apple", "Tablet"),
                    ["Samsung Galaxy Tab S9 Ultra"] = ("s9tab.png", "Samsung", "Tablet"),
                    ["Amazon Kindle Paperwhite"] = ("paperwrit.png", "Amazon", "Tablet"),
                    ["Lenovo Tab M10 Plus"] = ("lenovotab.png", "Lenovo", "Tablet"),
                    
                    // Laptops (3 products)
                    ["Apple MacBook Air 15? M2"] = ("macbook.png", "Apple", "Laptop"),
                    ["Dell XPS 13 Plus"] = ("dell.png", "Dell", "Laptop"),
                    ["ASUS ROG Zephyrus G16"] = ("rog.png", "ASUS", "Laptop"),
                    
                    // Accessories (8 products)
                    ["Sony WH-1000XM5"] = ("headsony.png", "Sony", "Accessory"),
                    ["Apple AirPods Pro (2nd Gen)"] = ("airpod.png", "Apple", "Accessory"),
                    ["Logitech MX Master 3S"] = ("mouse.png", "Logitech", "Accessory"),
                    ["Samsung T7 Shield SSD 1TB"] = ("ssd.png", "Samsung", "Accessory"),
                    ["Apple Watch Series 9"] = ("watch.png", "Apple", "Accessory"),
                    ["JBL Flip 6 Bluetooth Speaker"] = ("jbl.png", "JBL", "Accessory"),
                    ["Fitbit Charge 6"] = ("fitneswatch.png", "Fitbit", "Accessory"),
                    ["Bose QuietComfort Ultra"] = ("bose.png", "Bose", "Accessory"),
                    
                    // Gadgets (4 products)
                    ["GoPro HERO12 Black"] = ("gopro.png", "GoPro", "Gadget"),
                    ["DJI Mini 4 Pro"] = ("Drone.png", "DJI", "Gadget"),
                    ["Oculus Quest 3"] = ("vr.png", "Meta", "Gadget"),
                    ["Google Nest Hub (2nd Gen)"] = ("nesthub.png", "Google", "Gadget")
                };

                // Brand colors for professional image generation
                var brandColors = new Dictionary<string, Color>
                {
                    ["Apple"] = Color.FromArgb(26, 115, 232),
                    ["Samsung"] = Color.FromArgb(21, 101, 192),
                    ["Google"] = Color.FromArgb(66, 133, 244),
                    ["OnePlus"] = Color.FromArgb(255, 102, 0),
                    ["Amazon"] = Color.FromArgb(255, 153, 0),
                    ["Lenovo"] = Color.FromArgb(230, 0, 18),
                    ["Dell"] = Color.FromArgb(0, 123, 184),
                    ["ASUS"] = Color.FromArgb(255, 102, 0),
                    ["Sony"] = Color.FromArgb(0, 0, 0),
                    ["Logitech"] = Color.FromArgb(0, 184, 252),
                    ["JBL"] = Color.FromArgb(255, 105, 0),
                    ["Fitbit"] = Color.FromArgb(0, 184, 169),
                    ["Bose"] = Color.FromArgb(0, 0, 0),
                    ["GoPro"] = Color.FromArgb(0, 0, 0),
                    ["DJI"] = Color.FromArgb(252, 60, 60),
                    ["Meta"] = Color.FromArgb(24, 119, 242)
                };

                foreach (var product in products)
                {
                    if (imageSpecs.ContainsKey(product.Name))
                    {
                        var spec = imageSpecs[product.Name];
                        var imagePath = Path.Combine(imagesPath, spec.filename);
                        var localImageUrl = $"images/{spec.filename}";
                        
                        // Create physical image file with professional styling
                        if (!System.IO.File.Exists(imagePath))
                        {
                            await CreateProfessionalImageFile(imagePath, product.Name, spec.brand, spec.category, brandColors.GetValueOrDefault(spec.brand, Color.Gray));
                            createdFiles++;
                            _logger.LogInformation($"? Created professional image: {spec.filename} for {product.Name}");
                        }
                        
                        // Update database path
                        if (product.ImageUrl != localImageUrl)
                        {
                            var oldImageUrl = product.ImageUrl;
                            product.ImageUrl = localImageUrl;
                            product.UpdatedAt = DateTime.UtcNow;
                            updatedDatabase++;
                            _logger.LogInformation($"?? Updated database: {product.Name}: {oldImageUrl} ? {localImageUrl}");
                        }
                    }
                    else
                    {
                        // Create default image for unmapped products
                        var defaultImagePath = Path.Combine(imagesPath, "default-product.png");
                        if (!System.IO.File.Exists(defaultImagePath))
                        {
                            await CreateDefaultImageFile(defaultImagePath);
                            _logger.LogInformation($"? Created default image file");
                        }
                        
                        if (product.ImageUrl != "images/default-product.png")
                        {
                            product.ImageUrl = "images/default-product.png";
                            product.UpdatedAt = DateTime.UtcNow;
                            updatedDatabase++;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"?? Image creation complete! Created {createdFiles} files, updated {updatedDatabase} database entries");

                return Ok(new 
                { 
                    message = "Professional images created and database updated successfully!",
                    createdFiles = createdFiles,
                    updatedDatabaseEntries = updatedDatabase,
                    totalProducts = products.Count,
                    imagesDirectory = imagesPath,
                    timestamp = DateTime.UtcNow,
                    specification = "23 products with professional branding",
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error creating images: {ex.Message}");
                return StatusCode(500, new { error = "Failed to create images", message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // ? NEW: Create professional branded image files
        private async Task CreateProfessionalImageFile(string imagePath, string productName, string brand, string category, Color brandColor)
        {
            try
            {
                const int width = 400;
                const int height = 300;

                using (var bitmap = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // Set high quality rendering
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                    // Create gradient background
                    var lightColor = Color.FromArgb(brandColor.A, 
                        Math.Min(255, brandColor.R + 50), 
                        Math.Min(255, brandColor.G + 50), 
                        Math.Min(255, brandColor.B + 50));
                    
                    using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Rectangle(0, 0, width, height), lightColor, brandColor, 45f))
                    {
                        graphics.FillRectangle(gradientBrush, 0, 0, width, height);
                    }

                    // Add subtle pattern overlay
                    using (var overlayBrush = new SolidBrush(Color.FromArgb(20, Color.White)))
                    {
                        for (int i = 0; i < width; i += 40)
                        {
                            graphics.FillRectangle(overlayBrush, i, 0, 20, height);
                        }
                    }

                    // Draw product name
                    using (var titleFont = new Font("Arial", 18, FontStyle.Bold))
                    using (var titleBrush = new SolidBrush(Color.White))
                    {
                        var titleSize = graphics.MeasureString(productName, titleFont);
                        var titleX = (width - titleSize.Width) / 2;
                        var titleY = height / 2 - 30;
                        
                        // Add text shadow
                        using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                        {
                            graphics.DrawString(productName, titleFont, shadowBrush, titleX + 2, titleY + 2);
                        }
                        graphics.DrawString(productName, titleFont, titleBrush, titleX, titleY);
                    }

                    // Draw brand name
                    using (var brandFont = new Font("Arial", 14, FontStyle.Regular))
                    using (var brandBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                    {
                        var brandSize = graphics.MeasureString(brand, brandFont);
                        var brandX = (width - brandSize.Width) / 2;
                        var brandY = height / 2 + 10;
                        graphics.DrawString(brand, brandFont, brandBrush, brandX, brandY);
                    }

                    // Draw category badge
                    using (var categoryFont = new Font("Arial", 10, FontStyle.Bold))
                    using (var categoryBrush = new SolidBrush(brandColor))
                    using (var categoryTextBrush = new SolidBrush(Color.White))
                    {
                        var categorySize = graphics.MeasureString(category, categoryFont);
                        var badgeWidth = categorySize.Width + 20;
                        var badgeHeight = categorySize.Height + 8;
                        var badgeX = width - badgeWidth - 10;
                        var badgeY = 10;

                        var badgeRect = new Rectangle((int)badgeX, (int)badgeY, (int)badgeWidth, (int)badgeHeight);
                        graphics.FillRoundedRectangle(categoryBrush, badgeRect, 8);
                        graphics.DrawString(category, categoryFont, categoryTextBrush, badgeX + 10, badgeY + 4);
                    }

                    // Save as PNG
                    bitmap.Save(imagePath, ImageFormat.Png);
                }

                _logger.LogInformation($"?? Created professional image: {Path.GetFileName(imagePath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error creating image {imagePath}: {ex.Message}");
                // Fallback to simple placeholder
                await CreateSimpleImageFile(imagePath, productName, brand);
            }
        }

        // ? FALLBACK: Create simple image file if professional creation fails
        private async Task CreateSimpleImageFile(string imagePath, string productName, string brand)
        {
            try
            {
                // Create a simple 1x1 PNG as absolute fallback
                var pngData = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");
                await System.IO.File.WriteAllBytesAsync(imagePath, pngData);
                _logger.LogInformation($"?? Created fallback image: {Path.GetFileName(imagePath)}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error creating fallback image: {ex.Message}");
                throw;
            }
        }

        // ? Helper method for default product image
        private async Task CreateDefaultImageFile(string imagePath)
        {
            await CreateSimpleImageFile(imagePath, "Default Product", "Unknown");
        }

        // ? EXISTING METHODS (keeping for compatibility)
        [HttpPost("update-local-images")]
        public async Task<ActionResult> UpdateProductsWithLocalImages()
        {
            try
            {
                _logger.LogInformation("??? Updating products with local image paths...");

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
                        var oldUrl = product.ImageUrl;
                        product.ImageUrl = imageUpdates[product.Name];
                        product.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                        _logger.LogInformation($"? Updated: {product.Name}");
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Local image paths updated successfully!",
                    updatedCount = updatedCount,
                    totalProducts = products.Count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error updating local images: {ex.Message}");
                return StatusCode(500, new { error = "Failed to update images", message = ex.Message });
            }
        }

        [HttpPost("update-external-images")]
        public async Task<ActionResult> UpdateProductsWithExternalImages([FromBody] ExternalImageUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("?? API: Updating products with external image URLs...");
                
                var products = await _context.Products.ToListAsync();
                var updatedCount = 0;
                var errors = new List<string>();

                foreach (var imageUpdate in request.ImageUpdates)
                {
                    var product = products.FirstOrDefault(p => p.Name == imageUpdate.Key);
                    if (product != null)
                    {
                        var oldImageUrl = product.ImageUrl;
                        product.ImageUrl = imageUpdate.Value;
                        product.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                        
                        _logger.LogInformation($"? Updated {product.Name}: {oldImageUrl} ? {product.ImageUrl}");
                    }
                    else
                    {
                        var error = $"? Product not found: {imageUpdate.Key}";
                        errors.Add(error);
                        _logger.LogWarning(error);
                    }
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"?? Saved {updatedCount} product updates to database");
                }

                var result = new
                {
                    success = true,
                    message = $"Successfully updated {updatedCount} products with external image URLs",
                    updatedCount = updatedCount,
                    totalProducts = products.Count,
                    errors = errors,
                    timestamp = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error updating products with external images: {ex.Message}");
                return StatusCode(500, new { 
                    success = false, 
                    error = "Failed to update products with external images", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("update-to-external-urls")]
        public async Task<ActionResult> UpdateToExternalUrls()
        {
            try
            {
                _logger.LogInformation("?? API: Quick update to external URLs for all 23 products...");
                
                // Predefined external image URLs for all 23 products
                var externalImageUrls = new Dictionary<string, string>
                {
                    // ?? PHONES (4 products)
                    ["Apple iPhone 15 Pro"] = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/iphone-15-pro-model-unselect-gallery-2-202309?wid=5120&hei=2880&fmt=jpeg&qlt=80&.v=1692761460502",
                    ["Samsung Galaxy S24 Ultra"] = "https://www.wishque.com/data/images/products/11423/65395282_259007609966_0.66966800-1708668229.png",
                    ["Google Pixel 8 Pro"] = "https://xmobile.lk/wp-content/uploads/2023/10/3-31.jpg",
                    ["OnePlus 12"] = "https://technoor.me/wp-content/uploads/2023/12/oneplus-12-color_1.jpeg",
                    
                    // ?? TABLETS (4 products)
                    ["Apple iPad Pro 12.9? (M2)"] = "https://cdn.alloallo.media/catalog/product/apple/ipad/ipad-pro-12-9-in-6e-generation/ipad-pro-12-9-in-6e-generation-space-gray.jpg",
                    ["Samsung Galaxy Tab S9 Ultra"] = "https://images.samsung.com/is/image/samsung/p6pim/uk/2307/gallery/uk-galaxy-tab-s9-ultra-5g-x916-sm-x916bzaeeub-537349520?$624_624_PNG$",
                    ["Amazon Kindle Paperwhite"] = "https://m.media-amazon.com/images/I/81cOAQnitYL._UF1000,1000_QL80_.jpg",
                    ["Lenovo Tab M10 Plus"] = "https://p1-ofp.static.pub/medias/bWFzdGVyfHJvb3R8NTgzNDgwfGltYWdlL3BuZ3xoZjkvaDNjLzEzNjc0NDgwMTczMDg2LnBuZ3w3M2ZjZTJlOGJlYWQ3ZWZlYzJlZmI4NDg0ODhjMGI2ZTdjNzJmNDFlMTY5ZGQ0OTYwZGFjYmZiMmFmMzRhMDE4/lenovo-tab-m10-plus-gen-3-hero.png",
                    
                    // ?? LAPTOPS (3 products)
                    ["Apple MacBook Air 15? M2"] = "https://www.cnet.com/a/img/resize/b51c311f6732da72e77670beabcfcd07d39808ae/hub/2023/06/05/85a7355a-67e4-48a0-bd9c-2e927a3249b5/macbook-air-15-inch-m2-02.jpg?auto=webp&fit=crop&height=900&width=1200",
                    ["Dell XPS 13 Plus"] = "https://sm.pcmag.com/pcmag_au/review/d/dell-xps-1/dell-xps-13-plus-2023_8w51.jpg",
                    ["ASUS ROG Zephyrus G16"] = "https://m.media-amazon.com/images/I/61GkOVE3gnL._AC_SL1500_.jpg",
                    
                    // ?? ACCESSORIES (8 products)
                    ["Sony WH-1000XM5"] = "https://m.media-amazon.com/images/I/71o8Q5XJS5L._AC_SL1500_.jpg",
                    ["Apple AirPods Pro (2nd Gen)"] = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/MQD83_AV1?wid=1000&hei=1000&fmt=jpeg&qlt=80&.v=1660803973364",
                    ["Logitech MX Master 3S"] = "https://www.ubuy.com.lk/productimg/?image=aHR0cHM6Ly9tLW1lZGlhLWFtYXpvbi5jb20vaW1hZ2VzL0kvNjFuaTN0MXJ5UUwuX0FDX1NMMTUwMF8uanBn.jpg",
                    ["Samsung T7 Shield SSD 1TB"] = "https://www.barclays.lk/mmBC/Images/SSDS9146.JPG",
                    ["Apple Watch Series 9"] = "https://presentsolution.lk/wp-content/uploads/2024/02/series-9-45mm.jpg",
                    ["JBL Flip 6 Bluetooth Speaker"] = "https://trustedge.lk/wp-content/uploads/2024/01/2-26.png",
                    ["Fitbit Charge 6"] = "https://toyo.lk/wp-content/uploads/2023/12/Fitbit-Charge-6.jpg",
                    ["Bose QuietComfort Ultra"] = "https://m.media-amazon.com/images/I/51ZR4lyxBHL.jpg",
                    
                    // ?? GADGETS (4 products)
                    ["GoPro HERO12 Black"] = "https://rangashopping.lk/wp-content/uploads/2024/02/1693990916_IMG_2070536.jpg",
                    ["DJI Mini 4 Pro"] = "https://media.foto-erhardt.de/images/product_images/original_images/481/dji-mini-4-pro-fly-more-combo-dji-goggles-3-rc-motion-3-171377676348190304.jpg",
                    ["Oculus Quest 3"] = "https://m.media-amazon.com/images/I/51OT7thu1CL._UF1000,1000_QL80_.jpg",
                    ["Google Nest Hub (2nd Gen)"] = "https://www.gadgetguy.com.au/wp-content/uploads/2021/05/Nest-Hub_Lifestyle_Your-Evening-with-Sleep-Sensing.jpg"
                };

                var products = await _context.Products.ToListAsync();
                var updatedCount = 0;

                foreach (var imageUpdate in externalImageUrls)
                {
                    var product = products.FirstOrDefault(p => p.Name == imageUpdate.Key);
                    if (product != null)
                    {
                        var oldImageUrl = product.ImageUrl;
                        product.ImageUrl = imageUpdate.Value;
                        product.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                        
                        _logger.LogInformation($"? Updated {product.Name}");
                    }
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"?? Successfully updated {updatedCount} products with external image URLs");
                }

                return Ok(new 
                { 
                    success = true,
                    message = $"Successfully updated {updatedCount} products with external image URLs!",
                    updatedCount = updatedCount,
                    totalProducts = products.Count,
                    timestamp = DateTime.UtcNow,
                    imageUrlsType = "External"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error updating to external URLs: {ex.Message}");
                return StatusCode(500, new { 
                    success = false, 
                    error = "Failed to update to external URLs", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("switch-image-urls")]
        public async Task<ActionResult> SwitchImageUrls([FromBody] SwitchImageUrlsRequest request)
        {
            try
            {
                _logger.LogInformation($"?? API: Switching image URLs to {request.ImageUrlType} type...");
                
                var products = await _context.Products.ToListAsync();
                var updatedCount = 0;
                var errors = new List<string>();

                foreach (var product in products)
                {
                    var newImageUrl = GetImageUrlByType(product.Name, request.ImageUrlType);
                    if (newImageUrl != null && product.ImageUrl != newImageUrl)
                    {
                        var oldImageUrl = product.ImageUrl;
                        product.ImageUrl = newImageUrl;
                        product.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                        
                        _logger.LogInformation($"? Updated {product.Name}: {oldImageUrl} ? {newImageUrl}");
                    }
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"?? Successfully switched {updatedCount} products to {request.ImageUrlType} image URLs");
                }

                var result = new
                {
                    success = true,
                    message = $"Successfully switched {updatedCount} products to {request.ImageUrlType} image URLs",
                    updatedCount = updatedCount,
                    totalProducts = products.Count,
                    imageUrlType = request.ImageUrlType,
                    timestamp = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error switching image URLs: {ex.Message}");
                return StatusCode(500, new { 
                    success = false, 
                    error = "Failed to switch image URLs", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // ? NEW: Helper method to get image URL by type
        private string? GetImageUrlByType(string productName, string urlType)
        {
            // ? ENFORCED: Only external URLs are supported now
            return GetExternalImageUrl(productName);
        }

        // ? UPDATED: Get external image URL mapping - ONLY SOURCE OF TRUTH
        private string? GetExternalImageUrl(string productName)
        {
            var externalImageMappings = new Dictionary<string, string>
            {
                // ?? PHONES (4 products)
                ["Apple iPhone 15 Pro"] = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/iphone-15-pro-model-unselect-gallery-2-202309?wid=5120&hei=2880&fmt=jpeg&qlt=80&.v=1692761460502",
                ["Samsung Galaxy S24 Ultra"] = "https://www.wishque.com/data/images/products/11423/65395282_259007609966_0.66966800-1708668229.png",
                ["Google Pixel 8 Pro"] = "https://xmobile.lk/wp-content/uploads/2023/10/3-31.jpg",
                ["OnePlus 12"] = "https://technoor.me/wp-content/uploads/2023/12/oneplus-12-color_1.jpeg",
                
                // ?? TABLETS (4 products)
                ["Apple iPad Pro 12.9? (M2)"] = "https://cdn.alloallo.media/catalog/product/apple/ipad/ipad-pro-12-9-in-6e-generation/ipad-pro-12-9-in-6e-generation-space-gray.jpg",
                ["Samsung Galaxy Tab S9 Ultra"] = "https://images.samsung.com/is/image/samsung/p6pim/uk/2307/gallery/uk-galaxy-tab-s9-ultra-5g-x916-sm-x916bzaeeub-537349520?$624_624_PNG$",
                ["Amazon Kindle Paperwhite"] = "https://m.media-amazon.com/images/I/81cOAQnitYL._UF1000,1000_QL80_.jpg",
                ["Lenovo Tab M10 Plus"] = "https://p1-ofp.static.pub/medias/bWFzdGVyfHJvb3R8NTgzNDgwfGltYWdlL3BuZ3xoZjkvaDNjLzEzNjc0NDgwMTczMDg2LnBuZ3w3M2ZjZTJlOGJlYWQ3ZWZlYzJlZmI4NDg0ODhjMGI2ZTdjNzJmNDFlMTY5ZGQ0OTYwZGFjYmZiMmFmMzRhMDE4/lenovo-tab-m10-plus-gen-3-hero.png",
                
                // ?? LAPTOPS (3 products)
                ["Apple MacBook Air 15? M2"] = "https://www.cnet.com/a/img/resize/b51c311f6732da72e77670beabcfcd07d39808ae/hub/2023/06/05/85a7355a-67e4-48a0-bd9c-2e927a3249b5/macbook-air-15-inch-m2-02.jpg?auto=webp&fit=crop&height=900&width=1200",
                ["Dell XPS 13 Plus"] = "https://sm.pcmag.com/pcmag_au/review/d/dell-xps-1/dell-xps-13-plus-2023_8w51.jpg",
                ["ASUS ROG Zephyrus G16"] = "https://m.media-amazon.com/images/I/61GkOVE3gnL._AC_SL1500_.jpg",
                
                // ?? ACCESSORIES (8 products)
                ["Sony WH-1000XM5"] = "https://m.media-amazon.com/images/I/71o8Q5XJS5L._AC_SL1500_.jpg",
                ["Apple AirPods Pro (2nd Gen)"] = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/MQD83_AV1?wid=1000&hei=1000&fmt=jpeg&qlt=80&.v=1660803973364",
                ["Logitech MX Master 3S"] = "https://www.ubuy.com.lk/productimg/?image=aHR0cHM6Ly9tLW1lZGlhLWFtYXpvbi5jb20vaW1hZ2VzL0kvNjFuaTN0MXJ5UUwuX0FDX1NMMTUwMF8uanBn.jpg",
                ["Samsung T7 Shield SSD 1TB"] = "https://www.barclays.lk/mmBC/Images/SSDS9146.JPG",
                ["Apple Watch Series 9"] = "https://presentsolution.lk/wp-content/uploads/2024/02/series-9-45mm.jpg",
                ["JBL Flip 6 Bluetooth Speaker"] = "https://trustedge.lk/wp-content/uploads/2024/01/2-26.png",
                ["Fitbit Charge 6"] = "https://toyo.lk/wp-content/uploads/2023/12/Fitbit-Charge-6.jpg",
                ["Bose QuietComfort Ultra"] = "https://m.media-amazon.com/images/I/51ZR4lyxBHL.jpg",
                
                // ?? GADGETS (4 products)
                ["GoPro HERO12 Black"] = "https://rangashopping.lk/wp-content/uploads/2024/02/1693990916_IMG_2070536.jpg",
                ["DJI Mini 4 Pro"] = "https://media.foto-erhardt.de/images/product_images/original_images/481/dji-mini-4-pro-fly-more-combo-dji-goggles-3-rc-motion-3-171377676348190304.jpg",
                ["Oculus Quest 3"] = "https://m.media-amazon.com/images/I/51OT7thu1CL._UF1000,1000_QL80_.jpg",
                ["Google Nest Hub (2nd Gen)"] = "https://www.gadgetguy.com.au/wp-content/uploads/2021/05/Nest-Hub_Lifestyle_Your-Evening-with-Sleep-Sensing.jpg"
            };

            return externalImageMappings.ContainsKey(productName) ? externalImageMappings[productName] : 
                   // ? UPDATED: Fallback to external placeholder instead of local path
                   $"https://via.placeholder.com/400x300/667eea/ffffff?text={Uri.EscapeDataString(productName)}";
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducts(
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation($"?? API call: Search={search}, CategoryId={categoryId}, Page={page}");

                var query = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) ||
                                           (p.Description != null && p.Description.Contains(search)) ||
                                           (p.Brand != null && p.Brand.Contains(search)));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                var totalCount = await query.CountAsync();

                var products = await query
                    .OrderBy(p => p.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Brand,
                        p.Model,
                        p.ImageUrl,
                        Category = new
                        {
                            p.Category!.Id,
                            p.Category.Name
                        },
                        MinPrice = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Min(di => (decimal?)di.Price) ?? 299.99m,
                        MaxPrice = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Max(di => (decimal?)di.Price) ?? 299.99m,
                        AvailableStock = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Sum(di => di.Stock),
                        DistributorCount = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Count(),
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.IsActive
                    })
                    .ToListAsync();

                _logger.LogInformation($"? Returning {products.Count} products");

                return Ok(new
                {
                    products,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    retrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error in GetProducts: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        ProductCount = c.Products.Count(p => p.IsActive)
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error in GetCategories: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Id == id && p.IsActive)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Brand,
                        p.Model,
                        p.ImageUrl,
                        Category = new
                        {
                            p.Category.Id,
                            p.Category.Name,
                            p.Category.Description
                        },
                        CategoryId = p.CategoryId,
                        MinPrice = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Min(di => (decimal?)di.Price) ?? 299.99m,
                        MaxPrice = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Max(di => (decimal?)di.Price) ?? 299.99m,
                        AvailableStock = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Sum(di => di.Stock),
                        Pricing = _context.DistributorInventories
                            .Where(di => di.ProductId == p.Id && di.IsActive)
                            .Select(di => new
                            {
                                DistributorId = di.DistributorId,
                                DistributorName = di.Distributor.CompanyName,
                                Price = di.Price,
                                Stock = di.Stock,
                                DeliveryDays = di.DeliveryDays
                            })
                            .OrderBy(di => di.Price)
                            .ToList(),
                        p.CreatedAt,
                        p.UpdatedAt,
                        p.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error getting product {id}: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpGet("verify-external-images")]
        public async Task<ActionResult> VerifyExternalImages()
        {
            try
            {
                _logger.LogInformation("?? Verifying external image URLs in database...");

                var products = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Brand,
                        p.ImageUrl,
                        IsExternalUrl = p.ImageUrl != null && p.ImageUrl.StartsWith("http"),
                        IsLocalUrl = p.ImageUrl != null && p.ImageUrl.StartsWith("images/"),
                        p.UpdatedAt
                    })
                    .ToListAsync();

                var summary = new
                {
                    TotalProducts = products.Count,
                    ExternalImageUrls = products.Count(p => p.IsExternalUrl),
                    LocalImageUrls = products.Count(p => p.IsLocalUrl),
                    MissingImages = products.Count(p => string.IsNullOrEmpty(p.ImageUrl)),
                    LastUpdated = products.Max(p => p.UpdatedAt),
                    VerifiedAt = DateTime.UtcNow
                };

                _logger.LogInformation($"?? Image verification complete: {summary.ExternalImageUrls} external, {summary.LocalImageUrls} local, {summary.MissingImages} missing");

                return Ok(new
                {
                    Success = true,
                    Message = "Image verification completed successfully",
                    Summary = summary,
                    ProductDetails = products.Take(10), // First 10 products for verification
                    Recommendations = new[]
                    {
                        summary.ExternalImageUrls >= 20 ? "? Good: Most products have external image URLs" : "?? Warning: Few external image URLs found",
                        summary.MissingImages == 0 ? "? Good: No missing image URLs" : $"? Issue: {summary.MissingImages} products missing image URLs",
                        "?? External URLs provide high-quality images but may have CORS restrictions",
                        "?? Local URLs are faster but require image files in wwwroot/images/"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error verifying external images: {ex.Message}");
                return StatusCode(500, new { 
                    Success = false,
                    Error = "Failed to verify external images", 
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("test-external-image-loading")]
        public async Task<ActionResult> TestExternalImageLoading([FromBody] TestImageLoadingRequest request)
        {
            try
            {
                _logger.LogInformation("?? Testing external image loading for Razor Pages...");

                var imageTests = new List<object>();
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                // Get sample products with external URLs
                var sampleProducts = await _context.Products
                    .Where(p => p.IsActive && p.ImageUrl != null && p.ImageUrl.StartsWith("http"))
                    .Take(request.SampleSize ?? 5)
                    .Select(p => new { p.Name, p.ImageUrl })
                    .ToListAsync();

                foreach (var product in sampleProducts)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(product.ImageUrl, HttpCompletionOption.ResponseHeadersRead);
                        var isAccessible = response.IsSuccessStatusCode;
                        var contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
                        var contentLength = response.Content.Headers.ContentLength ?? 0;

                        imageTests.Add(new
                        {
                            ProductName = product.Name,
                            ImageUrl = product.ImageUrl,
                            IsAccessible = isAccessible,
                            StatusCode = (int)response.StatusCode,
                            ContentType = contentType,
                            ContentLength = contentLength,
                            ResponseTime = "< 5s"
                        });

                        _logger.LogInformation($"?? {product.Name}: {(isAccessible ? "?" : "?")} {response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        imageTests.Add(new
                        {
                            ProductName = product.Name,
                            ImageUrl = product.ImageUrl,
                            IsAccessible = false,
                            StatusCode = 0,
                            ContentType = "error",
                            ContentLength = 0,
                            ResponseTime = "timeout",
                            Error = ex.Message
                        });

                        _logger.LogWarning($"?? {product.Name}: Failed - {ex.Message}");
                    }
                }

                httpClient.Dispose();

                var successCount = imageTests.Count(t => (bool)t.GetType().GetProperty("IsAccessible")!.GetValue(t)!);
                var failureCount = imageTests.Count - successCount;

                return Ok(new
                {
                    Success = true,
                    Message = "External image loading test completed",
                    TestResults = new
                    {
                        TotalTested = imageTests.Count,
                        Successful = successCount,
                        Failed = failureCount,
                        SuccessRate = imageTests.Count > 0 ? (successCount * 100.0 / imageTests.Count) : 0
                    },
                    ImageTests = imageTests,
                    Recommendations = new[]
                    {
                        successCount > failureCount ? "? Most external images are accessible" : "?? Many external images are not accessible",
                        failureCount > 0 ? "?? Consider fallback mechanisms for failed images" : "? All tested images are loading properly",
                        "?? CORS restrictions may prevent images from loading in browser",
                        "?? Consider downloading images locally for best performance"
                    },
                    TestedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error testing external image loading: {ex.Message}");
                return StatusCode(500, new { 
                    Success = false,
                    Error = "Failed to test external image loading", 
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("image-status-dashboard")]
        public async Task<ActionResult> GetImageStatusDashboard()
        {
            try
            {
                _logger.LogInformation("?? Generating image status dashboard for Razor Pages...");

                var products = await _context.Products
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .ToListAsync();

                var dashboard = new
                {
                    Overview = new
                    {
                        TotalProducts = products.Count,
                        ExternalImages = products.Count(p => p.ImageUrl?.StartsWith("http") == true),
                        LocalImages = products.Count(p => p.ImageUrl?.StartsWith("images/") == true),
                        MissingImages = products.Count(p => string.IsNullOrEmpty(p.ImageUrl)),
                        LastUpdated = products.Max(p => p.UpdatedAt)
                    },
                    CategoryBreakdown = products
                        .GroupBy(p => p.Category.Name)
                        .Select(g => new
                        {
                            Category = g.Key,
                            TotalProducts = g.Count(),
                            ExternalImages = g.Count(p => p.ImageUrl?.StartsWith("http") == true),
                            LocalImages = g.Count(p => p.ImageUrl?.StartsWith("images/") == true),
                            MissingImages = g.Count(p => string.IsNullOrEmpty(p.ImageUrl))
                        })
                        .OrderBy(c => c.Category)
                        .ToList(),
                    RecentUpdates = products
                        .Where(p => p.UpdatedAt > DateTime.UtcNow.AddDays(-7))
                        .OrderByDescending(p => p.UpdatedAt)
                        .Take(10)
                        .Select(p => new
                        {
                            p.Name,
                            p.ImageUrl,
                            ImageType = p.ImageUrl?.StartsWith("http") == true ? "External" : 
                                       p.ImageUrl?.StartsWith("images/") == true ? "Local" : "Missing",
                            p.UpdatedAt
                        })
                        .ToList(),
                    Status = new
                    {
                        DatabaseHealth = "? Connected",
                        ImageConfiguration = products.Count(p => !string.IsNullOrEmpty(p.ImageUrl)) > products.Count * 0.8 ? 
                            "? Good" : "?? Needs Attention",
                        ExternalUrlRatio = products.Count > 0 ? 
                            $"{products.Count(p => p.ImageUrl?.StartsWith("http") == true) * 100 / products.Count}%" : "0%",
                        GeneratedAt = DateTime.UtcNow
                    }
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError($"? Error generating image status dashboard: {ex.Message}");
                return StatusCode(500, new { 
                    Error = "Failed to generate dashboard", 
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    // ? Helper extension for rounded rectangles
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle rect, int radius)
        {
            try
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    graphics.FillPath(brush, path);
                }
            }
            catch
            {
                // Fallback to regular rectangle if rounded corners fail
                graphics.FillRectangle(brush, rect);
            }
        }
    }

    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class ExternalImageUpdateRequest
    {
        public Dictionary<string, string> ImageUpdates { get; set; } = new();
    }

    public class TestImageLoadingRequest
    {
        public int? SampleSize { get; set; } = 5;
        public bool IncludeResponseTime { get; set; } = true;
        public bool CheckContentType { get; set; } = true;
    }

    public class SwitchImageUrlsRequest
    {
        public string ImageUrlType { get; set; } = "local"; // "local" or "external"
    }
}