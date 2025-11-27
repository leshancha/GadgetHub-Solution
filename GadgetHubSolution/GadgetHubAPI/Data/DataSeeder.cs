using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GadgetHubAPI.Data
{
    public class DataSeeder
    {
        // ✅ UPDATED: Configuration for image URL type with External as default
        public enum ImageUrlType
        {
            External, // Default: External URLs only
            Local     // Legacy: Local paths (deprecated)
        }

        public static async Task SeedAsync(ApplicationDbContext context, ImageUrlType imageUrlType = ImageUrlType.External)
        {
            try
            {
                // ✅ ENHANCED: Always ensure database is created first
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("✅ Database ensured to exist");

                // ✅ ENHANCED: Check existing data and provide better logging
                var existingCustomers = await context.Customers.CountAsync();
                var existingProducts = await context.Products.CountAsync();
                var existingCategories = await context.Categories.CountAsync();
                var existingQuotations = await context.QuotationRequests.CountAsync();
                
                Console.WriteLine($"📊 Current database state:");
                Console.WriteLine($"   👤 Customers: {existingCustomers}");
                Console.WriteLine($"   📦 Products: {existingProducts}");
                Console.WriteLine($"   📁 Categories: {existingCategories}");
                Console.WriteLine($"   📋 Quotation Requests: {existingQuotations}");

                // ✅ ENHANCED: Only clear and reseed if explicitly needed or if data is incomplete
                if (existingCustomers > 0 && existingProducts > 0 && existingCategories > 0)
                {
                    Console.WriteLine($"✅ Database already contains complete base data - updating image URLs to {imageUrlType}");
                    
                    // ✅ NEW: Update image URLs based on type preference
                    await UpdateProductImageUrls(context, imageUrlType);
                    
                    // ✅ ADDED: Verify the test customer exists
                    var testCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Email == "customer@test.com");
                    if (testCustomer != null)
                    {
                        Console.WriteLine($"✅ Test customer verified: ID={testCustomer.Id}, Name={testCustomer.Name}, Active={testCustomer.IsActive}");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Test customer missing - will create essential accounts");
                        await EnsureTestAccountsExist(context);
                    }
                    
                    Console.WriteLine($"📋 Preserving existing {existingQuotations} quotation requests");
                    return;
                }
                else
                {
                    Console.WriteLine("🗑️ Clearing existing incomplete data for fresh seed...");
                    
                    // Only clear when doing full reseed
                    await ClearAllQuotationData(context);
                    
                    // Clear all existing data in the correct order (respecting foreign keys)
                    context.DistributorInventories.RemoveRange(context.DistributorInventories);
                    context.Products.RemoveRange(context.Products);
                    context.Categories.RemoveRange(context.Categories);
                    context.Distributors.RemoveRange(context.Distributors);
                    context.Customers.RemoveRange(context.Customers);
                    context.Admins.RemoveRange(context.Admins);
                    
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Existing data cleared successfully");
                }

                // ✅ ENHANCED: Always ensure test accounts exist
                await EnsureTestAccountsExist(context);

                // Only do full seeding if we cleared everything
                if (existingCategories == 0)
                {
                    await PerformFullSeeding(context, imageUrlType);
                }

                Console.WriteLine($"✅ Database seeding completed successfully by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine("📋 Quotation data preservation enabled - real customer quotations will be maintained");
                Console.WriteLine($"🖼️ All images using {imageUrlType} URLs (External URLs ONLY - Local paths removed)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding database: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // ✅ NEW: Update product image URLs based on type
        public static async Task UpdateProductImageUrls(ApplicationDbContext context, ImageUrlType imageUrlType)
        {
            try
            {
                Console.WriteLine($"🔄 Updating all product image URLs to {imageUrlType} type...");
                
                var allProducts = await context.Products.ToListAsync();
                var updatedCount = 0;

                foreach (var product in allProducts)
                {
                    var newImageUrl = GetImageUrl(product.Name, imageUrlType);
                    if (newImageUrl != null && product.ImageUrl != newImageUrl)
                    {
                        var oldUrl = product.ImageUrl;
                        product.ImageUrl = newImageUrl;
                        product.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                        
                        Console.WriteLine($"🔄 Updated: {product.Name}");
                        Console.WriteLine($"   From: {oldUrl ?? "null"}");
                        Console.WriteLine($"   To: {newImageUrl}");
                    }
                }

                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Updated {updatedCount} product image URLs to {imageUrlType} type");
                }
                else
                {
                    Console.WriteLine($"✅ All products already have {imageUrlType} image URLs");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating image URLs: {ex.Message}");
                throw;
            }
        }

        // ✅ UPDATED: Get image URL based on type and product name - External URLs prioritized
        private static string? GetImageUrl(string productName, ImageUrlType imageUrlType)
        {
            return imageUrlType switch
            {
                ImageUrlType.External => GetExternalImageUrl(productName),
                ImageUrlType.Local => GetExternalImageUrl(productName), // ✅ UPDATED: Always use external URLs
                _ => GetExternalImageUrl(productName)
            };
        }

        // ✅ UPDATED: Get external image URL for a product - ONLY SOURCE OF TRUTH
        private static string? GetExternalImageUrl(string productName)
        {
            var externalImageMappings = new Dictionary<string, string>
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

            return externalImageMappings.ContainsKey(productName) ? externalImageMappings[productName] : 
                   // Fallback to external placeholder instead of local path
                   $"https://via.placeholder.com/400x300/667eea/ffffff?text={Uri.EscapeDataString(productName)}";
        }

        // ✅ REMOVED: CleanExternalImageUrls method (no longer needed)

        // ✅ NEW: Method to clear ALL quotation data
        private static async Task ClearAllQuotationData(ApplicationDbContext context)
        {
            try
            {
                // Clear quotation data in correct order to respect foreign key constraints
                var quotationResponseItems = await context.QuotationResponseItems.ToListAsync();
                if (quotationResponseItems.Any())
                {
                    context.QuotationResponseItems.RemoveRange(quotationResponseItems);
                    Console.WriteLine($"🗑️ Cleared {quotationResponseItems.Count} quotation response items");
                }

                var quotationResponses = await context.QuotationResponses.ToListAsync();
                if (quotationResponses.Any())
                {
                    context.QuotationResponses.RemoveRange(quotationResponses);
                    Console.WriteLine($"🗑️ Cleared {quotationResponses.Count} quotation responses");
                }

                var quotationRequestItems = await context.QuotationRequestItems.ToListAsync();
                if (quotationRequestItems.Any())
                {
                    context.QuotationRequestItems.RemoveRange(quotationRequestItems);
                    Console.WriteLine($"🗑️ Cleared {quotationRequestItems.Count} quotation request items");
                }

                var quotationRequests = await context.QuotationRequests.ToListAsync();
                if (quotationRequests.Any())
                {
                    context.QuotationRequests.RemoveRange(quotationRequests);
                    Console.WriteLine($"🗑️ Cleared {quotationRequests.Count} quotation requests");
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ All quotation data cleared successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error clearing quotation data: {ex.Message}");
                throw;
            }
        }

        // ✅ ADDED: Ensure test accounts always exist
        private static async Task EnsureTestAccountsExist(ApplicationDbContext context)
        {
            // Ensure Admin exists
            var admin = await context.Admins.FirstOrDefaultAsync(a => a.Email == "admin@gadgethub.com");
            if (admin == null)
            {
                admin = new Admin
                {
                    Username = "admin",
                    FullName = "System Administrator",
                    Email = "admin@gadgethub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", 12),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Admins.AddAsync(admin);
                Console.WriteLine("✅ Created admin account");
            }

            // Ensure Test Customer exists
            var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email == "customer@test.com");
            if (customer == null)
            {
                customer = new Customer
                {
                    Name = "Test Customer",
                    Email = "customer@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123", 12),
                    Phone = "+1234567890",
                    Address = "123 Customer Street, Tech City, TC 12345",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Customers.AddAsync(customer);
                Console.WriteLine("✅ Created test customer account");
            }

            // Ensure Test Distributor exists
            var distributor = await context.Distributors.FirstOrDefaultAsync(d => d.Email == "tech@dis.com");
            if (distributor == null)
            {
                distributor = new Distributor
                {
                    CompanyName = "TechWorld",
                    Email = "tech@dis.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("tech123", 12),
                    ContactPerson = "Tech World",
                    Phone = "07777777",
                    Address = "456 Tech Boulevard, Colombo",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Distributors.AddAsync(distributor);
                Console.WriteLine("✅ Created test distributor account");
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Essential test accounts verified/created");
        }

        // ✅ ENHANCED: Separate full seeding logic with external URLs only
        private static async Task PerformFullSeeding(ApplicationDbContext context, ImageUrlType imageUrlType)
        {
            Console.WriteLine($"🌱 Performing full database seeding with {imageUrlType} image URLs...");

            // 1. Seed Categories first
            var categories = new List<Category>
            {
                new() { Name = "Phone", Description = "Smartphones and mobile devices" },
                new() { Name = "Tablet", Description = "Tablets and e-readers" },
                new() { Name = "Laptop", Description = "Laptops and notebooks" },
                new() { Name = "Accessory", Description = "Device accessories and peripherals" },
                new() { Name = "Gadget", Description = "Smart gadgets and wearables" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Get the actual IDs assigned by the database
            var phoneCategory = await context.Categories.FirstAsync(c => c.Name == "Phone");
            var tabletCategory = await context.Categories.FirstAsync(c => c.Name == "Tablet");
            var laptopCategory = await context.Categories.FirstAsync(c => c.Name == "Laptop");
            var accessoryCategory = await context.Categories.FirstAsync(c => c.Name == "Accessory");
            var gadgetCategory = await context.Categories.FirstAsync(c => c.Name == "Gadget");

            // ✅ YOUR EXACT 23 PRODUCTS WITH EXTERNAL IMAGE URLS ONLY
            var products = new List<Product>
            {
                // Phones (4 products) - IDs 1-4
                new() { Name = "Apple iPhone 15 Pro", Description = "Apple's 2023 flagship, 6.1″ OLED, A17 Pro chip.", CategoryId = phoneCategory.Id, Brand = "Apple", Model = "A3101", ImageUrl = GetImageUrl("Apple iPhone 15 Pro", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Samsung Galaxy S24 Ultra", Description = "Samsung flagship, 6.8″ QHD+, Snapdragon 8 Gen 3.", CategoryId = phoneCategory.Id, Brand = "Samsung", Model = "SM-S928B", ImageUrl = GetImageUrl("Samsung Galaxy S24 Ultra", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Google Pixel 8 Pro", Description = "Google's AI-powered flagship, 6.7″ OLED, Tensor G3.", CategoryId = phoneCategory.Id, Brand = "Google", Model = "GA03210-US", ImageUrl = GetImageUrl("Google Pixel 8 Pro", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "OnePlus 12", Description = "Fast & smooth, 6.8″ AMOLED, Snapdragon 8 Gen 3.", CategoryId = phoneCategory.Id, Brand = "OnePlus", Model = "CPH2581", ImageUrl = GetImageUrl("OnePlus 12", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },

                // Tablets (4 products) - IDs 5-8
                new() { Name = "Apple iPad Pro 12.9″ (M2)", Description = "Latest iPad Pro, 12.9″ Liquid Retina XDR, M2 chip.", CategoryId = tabletCategory.Id, Brand = "Apple", Model = "MTHQ3", ImageUrl = GetImageUrl("Apple iPad Pro 12.9″ (M2)", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Samsung Galaxy Tab S9 Ultra", Description = "14.6″ Dynamic AMOLED, S Pen, Snapdragon 8 Gen 2.", CategoryId = tabletCategory.Id, Brand = "Samsung", Model = "SM-X916B", ImageUrl = GetImageUrl("Samsung Galaxy Tab S9 Ultra", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Amazon Kindle Paperwhite", Description = "E-ink 6.8″ reader, warm light, waterproof.", CategoryId = tabletCategory.Id, Brand = "Amazon", Model = "B08KTZ8249", ImageUrl = GetImageUrl("Amazon Kindle Paperwhite", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Lenovo Tab M10 Plus", Description = "10.6″ FHD, lightweight Android tablet.", CategoryId = tabletCategory.Id, Brand = "Lenovo", Model = "TB128FU", ImageUrl = GetImageUrl("Lenovo Tab M10 Plus", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },

                // Laptops (3 products) - IDs 9-11
                new() { Name = "Apple MacBook Air 15″ M2", Description = "15″ Liquid Retina, M2 chip, ultralight.", CategoryId = laptopCategory.Id, Brand = "Apple", Model = "MQKW3", ImageUrl = GetImageUrl("Apple MacBook Air 15″ M2", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Dell XPS 13 Plus", Description = "13.4″ OLED, Intel Core Ultra 7, 32GB RAM.", CategoryId = laptopCategory.Id, Brand = "Dell", Model = "9320", ImageUrl = GetImageUrl("Dell XPS 13 Plus", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "ASUS ROG Zephyrus G16", Description = "16″ QHD, RTX 4070, gaming powerhouse.", CategoryId = laptopCategory.Id, Brand = "ASUS", Model = "GU605MZ", ImageUrl = GetImageUrl("ASUS ROG Zephyrus G16", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },

                // Accessories (8 products) - IDs 12-19
                new() { Name = "Sony WH-1000XM5", Description = "Premium noise-cancelling headphones.", CategoryId = accessoryCategory.Id, Brand = "Sony", Model = "WH1000XM5", ImageUrl = GetImageUrl("Sony WH-1000XM5", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Apple AirPods Pro (2nd Gen)", Description = "Active noise cancelling, MagSafe case.", CategoryId = accessoryCategory.Id, Brand = "Apple", Model = "MTJV3", ImageUrl = GetImageUrl("Apple AirPods Pro (2nd Gen)", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Logitech MX Master 3S", Description = "Ergonomic, wireless mouse for productivity.", CategoryId = accessoryCategory.Id, Brand = "Logitech", Model = "910-006556", ImageUrl = GetImageUrl("Logitech MX Master 3S", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Samsung T7 Shield SSD 1TB", Description = "Portable SSD, rugged, USB 3.2.", CategoryId = accessoryCategory.Id, Brand = "Samsung", Model = "MU-PE1T0S", ImageUrl = GetImageUrl("Samsung T7 Shield SSD 1TB", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Apple Watch Series 9", Description = "45mm, always-on Retina, fitness tracking.", CategoryId = accessoryCategory.Id, Brand = "Apple", Model = "MXL73", ImageUrl = GetImageUrl("Apple Watch Series 9", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "JBL Flip 6 Bluetooth Speaker", Description = "Waterproof, portable, bold sound.", CategoryId = accessoryCategory.Id, Brand = "JBL", Model = "JBLFLIP6", ImageUrl = GetImageUrl("JBL Flip 6 Bluetooth Speaker", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Fitbit Charge 6", Description = "Health tracker, ECG, GPS.", CategoryId = accessoryCategory.Id, Brand = "Fitbit", Model = "FB421", ImageUrl = GetImageUrl("Fitbit Charge 6", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Bose QuietComfort Ultra", Description = "Flagship over-ear headphones, ANC.", CategoryId = accessoryCategory.Id, Brand = "Bose", Model = "QCUH", ImageUrl = GetImageUrl("Bose QuietComfort Ultra", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },

                // Gadgets (4 products) - IDs 20-23 
                new() { Name = "GoPro HERO12 Black", Description = "Waterproof 5.3K action camera.", CategoryId = gadgetCategory.Id, Brand = "GoPro", Model = "CHDHX-121", ImageUrl = GetImageUrl("GoPro HERO12 Black", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "DJI Mini 4 Pro", Description = "Ultralight 4K drone with 48MP camera.", CategoryId = gadgetCategory.Id, Brand = "DJI", Model = "CP.MA.00000692.01", ImageUrl = GetImageUrl("DJI Mini 4 Pro", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Oculus Quest 3", Description = "Standalone VR headset by Meta.", CategoryId = gadgetCategory.Id, Brand = "Meta", Model = "899-00583-01", ImageUrl = GetImageUrl("Oculus Quest 3", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
                new() { Name = "Google Nest Hub (2nd Gen)", Description = "7″ smart display with Google Assistant.", CategoryId = gadgetCategory.Id, Brand = "Google", Model = "GA01331-US", ImageUrl = GetImageUrl("Google Nest Hub (2nd Gen)", imageUrlType), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Added {products.Count} products from YOUR EXACT SPECIFICATION with {imageUrlType} IMAGE URLS");
            Console.WriteLine($"🖼️ All products now using {imageUrlType} image URLs (EXTERNAL ONLY)");
            Console.WriteLine("📱 4 Phones | 📱 4 Tablets | 💻 3 Laptops | 🎧 8 Accessories | 🎮 4 Gadgets = 23 Total");

            // ✅ ENHANCED: Ensure distributor inventory is created for ALL products
            await SeedDistributorInventory(context);
        }

        // ✅ REMOVED: GetLocalImagePath method (local paths no longer supported)

        // ✅ NEW: Separate method to ensure comprehensive distributor inventory
        private static async Task SeedDistributorInventory(ApplicationDbContext context)
        {
            var allProducts = await context.Products.ToListAsync();
            var allDistributors = await context.Distributors.ToListAsync();

            if (!allDistributors.Any() || !allProducts.Any())
            {
                Console.WriteLine("⚠️ No distributors or products found - skipping inventory seeding");
                return;
            }

            // Clear existing inventory to avoid duplicates
            var existingInventory = await context.DistributorInventories.ToListAsync();
            if (existingInventory.Any())
            {
                context.DistributorInventories.RemoveRange(existingInventory);
                await context.SaveChangesAsync();
                Console.WriteLine("🗑️ Cleared existing distributor inventory");
            }

            var inventoryItems = new List<DistributorInventory>();

            foreach (var distributor in allDistributors)
            {
                foreach (var product in allProducts)
                {
                    var basePrice = GetBasePrice(product.Name);
                    // Create some price variation between distributors
                    var variation = (distributor.Id - 1) * 0.05m + ((decimal)new Random(product.Id + distributor.Id).NextSingle() * 0.1m);
                    var finalPrice = basePrice * (1 + variation);

                    inventoryItems.Add(new DistributorInventory
                    {
                        DistributorId = distributor.Id,
                        ProductId = product.Id,
                        Price = Math.Round(finalPrice, 2),
                        Stock = new Random(product.Id + distributor.Id).Next(50, 200),
                        DeliveryDays = new Random(product.Id + distributor.Id * 2).Next(1, 14),
                        IsActive = true,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            await context.DistributorInventories.AddRangeAsync(inventoryItems);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Added {inventoryItems.Count} distributor inventory items ({allDistributors.Count} distributors × {allProducts.Count} products)");
        }

        private static decimal GetBasePrice(string productName)
        {
            return productName switch
            {
                // Phones (4 products) - YOUR EXACT SPECIFICATION PRICING
                "Apple iPhone 15 Pro" => 999.00m,
                "Samsung Galaxy S24 Ultra" => 1199.00m,
                "Google Pixel 8 Pro" => 899.00m,
                "OnePlus 12" => 799.00m,
                
                // Tablets (4 products) - YOUR EXACT SPECIFICATION PRICING
                "Apple iPad Pro 12.9″ (M2)" => 1299.00m,
                "Samsung Galaxy Tab S9 Ultra" => 1199.00m,
                "Amazon Kindle Paperwhite" => 149.00m,
                "Lenovo Tab M10 Plus" => 299.00m,
                
                // Laptops (3 products) - YOUR EXACT SPECIFICATION PRICING
                "Apple MacBook Air 15″ M2" => 1499.00m,
                "Dell XPS 13 Plus" => 1299.00m,
                "ASUS ROG Zephyrus G16" => 2499.00m,
                
                // Accessories (8 products) - YOUR EXACT SPECIFICATION PRICING
                "Sony WH-1000XM5" => 349.00m,
                "Apple AirPods Pro (2nd Gen)" => 249.00m,
                "Logitech MX Master 3S" => 99.00m,
                "Samsung T7 Shield SSD 1TB" => 159.00m,
                "Apple Watch Series 9" => 399.00m,
                "JBL Flip 6 Bluetooth Speaker" => 119.00m,
                "Fitbit Charge 6" => 199.00m,
                "Bose QuietComfort Ultra" => 429.00m,
                
                // Gadgets (4 products) - YOUR EXACT SPECIFICATION PRICING
                "GoPro HERO12 Black" => 399.00m,
                "DJI Mini 4 Pro" => 759.00m,
                "Oculus Quest 3" => 499.00m,
                "Google Nest Hub (2nd Gen)" => 99.00m,
                
                _ => 299.00m // Default price for any unlisted products
            };
        }
    }
}