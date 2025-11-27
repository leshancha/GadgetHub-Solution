using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Check if we already have data
                if (await context.Products.AnyAsync())
                {
                    Console.WriteLine("✅ Database already seeded with products");
                    return;
                }

                Console.WriteLine("🌱 Seeding database with YOUR EXACT 23 PRODUCTS...");
                Console.WriteLine($"👤 Seeding initiated by: leshancha");
                Console.WriteLine($"⏰ Seeding started at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                // Seed Categories first - Using your exact category names
                var categories = new List<Category>
                {
                    new Category { Name = "Phone", Description = "Smartphones and mobile devices" },
                    new Category { Name = "Tablet", Description = "Tablets and e-readers" },
                    new Category { Name = "Laptop", Description = "Laptops and notebooks" },
                    new Category { Name = "Accessory", Description = "Device accessories and peripherals" },
                    new Category { Name = "Gadget", Description = "Smart gadgets and wearables" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Get actual category IDs from database
                var phoneCategory = await context.Categories.FirstAsync(c => c.Name == "Phone");
                var tabletCategory = await context.Categories.FirstAsync(c => c.Name == "Tablet");
                var laptopCategory = await context.Categories.FirstAsync(c => c.Name == "Laptop");
                var accessoryCategory = await context.Categories.FirstAsync(c => c.Name == "Accessory");
                var gadgetCategory = await context.Categories.FirstAsync(c => c.Name == "Gadget");

                // ✅ YOUR EXACT 23 PRODUCTS WITH LOCAL PATHS ONLY
                var products = new List<Product>
                {
                    // Phones (4 products)
                    new Product
                    {
                        Name = "Apple iPhone 15 Pro",
                        Description = "Apple's 2023 flagship, 6.1″ OLED, A17 Pro chip.",
                        CategoryId = phoneCategory.Id,
                        Brand = "Apple",
                        Model = "A3101",
                        ImageUrl = "images/Apple15.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy S24 Ultra",
                        Description = "Samsung flagship, 6.8″ QHD+, Snapdragon 8 Gen 3.",
                        CategoryId = phoneCategory.Id,
                        Brand = "Samsung",
                        Model = "SM-S928B",
                        ImageUrl = "images/s24ultra.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Google Pixel 8 Pro",
                        Description = "Google's AI-powered flagship, 6.7″ OLED, Tensor G3.",
                        CategoryId = phoneCategory.Id,
                        Brand = "Google",
                        Model = "GA03210-US",
                        ImageUrl = "images/pixel.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "OnePlus 12",
                        Description = "Fast & smooth, 6.8″ AMOLED, Snapdragon 8 Gen 3.",
                        CategoryId = phoneCategory.Id,
                        Brand = "OnePlus",
                        Model = "CPH2581",
                        ImageUrl = "images/oneplus.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },

                    // Tablets (4 products)
                    new Product
                    {
                        Name = "Apple iPad Pro 12.9″ (M2)",
                        Description = "Latest iPad Pro, 12.9″ Liquid Retina XDR, M2 chip.",
                        CategoryId = tabletCategory.Id,
                        Brand = "Apple",
                        Model = "MTHQ3",
                        ImageUrl = "images/ipad.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy Tab S9 Ultra",
                        Description = "14.6″ Dynamic AMOLED, S Pen, Snapdragon 8 Gen 2.",
                        CategoryId = tabletCategory.Id,
                        Brand = "Samsung",
                        Model = "SM-X916B",
                        ImageUrl = "images/s9tab.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Amazon Kindle Paperwhite",
                        Description = "E-ink 6.8″ reader, warm light, waterproof.",
                        CategoryId = tabletCategory.Id,
                        Brand = "Amazon",
                        Model = "B08KTZ8249",
                        ImageUrl = "images/paperwrit.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Lenovo Tab M10 Plus",
                        Description = "10.6″ FHD, lightweight Android tablet.",
                        CategoryId = tabletCategory.Id,
                        Brand = "Lenovo",
                        Model = "TB128FU",
                        ImageUrl = "images/lenovotab.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },

                    // Laptops (3 products)
                    new Product
                    {
                        Name = "Apple MacBook Air 15″ M2",
                        Description = "15″ Liquid Retina, M2 chip, ultralight.",
                        CategoryId = laptopCategory.Id,
                        Brand = "Apple",
                        Model = "MQKW3",
                        ImageUrl = "images/macbook.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Dell XPS 13 Plus",
                        Description = "13.4″ OLED, Intel Core Ultra 7, 32GB RAM.",
                        CategoryId = laptopCategory.Id,
                        Brand = "Dell",
                        Model = "9320",
                        ImageUrl = "images/dell.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "ASUS ROG Zephyrus G16",
                        Description = "16″ QHD, RTX 4070, gaming powerhouse.",
                        CategoryId = laptopCategory.Id,
                        Brand = "ASUS",
                        Model = "GU605MZ",
                        ImageUrl = "images/rog.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },

                    // Accessories (8 products)
                    new Product
                    {
                        Name = "Sony WH-1000XM5",
                        Description = "Premium noise-cancelling headphones.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "Sony",
                        Model = "WH1000XM5",
                        ImageUrl = "images/headsony.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Apple AirPods Pro (2nd Gen)",
                        Description = "Active noise cancelling, MagSafe case.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "Apple",
                        Model = "MTJV3",
                        ImageUrl = "images/airpod.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Logitech MX Master 3S",
                        Description = "Ergonomic, wireless mouse for productivity.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "Logitech",
                        Model = "910-006556",
                        ImageUrl = "images/mouse.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Samsung T7 Shield SSD 1TB",
                        Description = "Portable SSD, rugged, USB 3.2.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "Samsung",
                        Model = "MU-PE1T0S",
                        ImageUrl = "images/ssd.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Apple Watch Series 9",
                        Description = "45mm, always-on Retina, fitness tracking.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "Apple",
                        Model = "MXL73",
                        ImageUrl = "images/watch.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "JBL Flip 6 Bluetooth Speaker",
                        Description = "Waterproof, portable, bold sound.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "JBL",
                        Model = "JBLFLIP6",
                        ImageUrl = "images/jbl.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Fitbit Charge 6",
                        Description = "Health tracker, ECG, GPS.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "Fitbit",
                        Model = "FB421",
                        ImageUrl = "images/fitneswatch.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Bose QuietComfort Ultra",
                        Description = "Flagship over-ear headphones, ANC.",
                        CategoryId = accessoryCategory.Id,
                        Brand = "Bose",
                        Model = "QCUH",
                        ImageUrl = "images/bose.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },

                    // Gadgets (4 products)
                    new Product
                    {
                        Name = "GoPro HERO12 Black",
                        Description = "Waterproof 5.3K action camera.",
                        CategoryId = gadgetCategory.Id,
                        Brand = "GoPro",
                        Model = "CHDHX-121",
                        ImageUrl = "images/gopro.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "DJI Mini 4 Pro",
                        Description = "Ultralight 4K drone with 48MP camera.",
                        CategoryId = gadgetCategory.Id,
                        Brand = "DJI",
                        Model = "CP.MA.00000692.01",
                        ImageUrl = "images/Drone.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Oculus Quest 3",
                        Description = "Standalone VR headset by Meta.",
                        CategoryId = gadgetCategory.Id,
                        Brand = "Meta",
                        Model = "899-00583-01",
                        ImageUrl = "images/vr.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Google Nest Hub (2nd Gen)",
                        Description = "7″ smart display with Google Assistant.",
                        CategoryId = gadgetCategory.Id,
                        Brand = "Google",
                        Model = "GA01331-US",
                        ImageUrl = "images/nesthub.png",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();

                Console.WriteLine("✅ Successfully seeded YOUR EXACT 23 PRODUCTS with LOCAL PATHS!");
                Console.WriteLine($"📱 {products.Count(p => p.CategoryId == phoneCategory.Id)} Phones");
                Console.WriteLine($"📱 {products.Count(p => p.CategoryId == tabletCategory.Id)} Tablets");
                Console.WriteLine($"💻 {products.Count(p => p.CategoryId == laptopCategory.Id)} Laptops");
                Console.WriteLine($"🎧 {products.Count(p => p.CategoryId == accessoryCategory.Id)} Accessories");
                Console.WriteLine($"🎮 {products.Count(p => p.CategoryId == gadgetCategory.Id)} Gadgets");
                Console.WriteLine($"🖼️ ALL using LOCAL image paths from GadgetHubWeb/wwwroot/images/");
                Console.WriteLine($"👤 Database seeded successfully by: leshancha");
                Console.WriteLine($"⏰ Seeding completed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding database: {ex.Message}");
                Console.WriteLine($"👤 Error reported by: leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                throw;
            }
        }
    }
}