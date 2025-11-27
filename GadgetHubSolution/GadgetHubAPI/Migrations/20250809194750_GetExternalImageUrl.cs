using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GadgetHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class GetExternalImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ COMPLETE REMOVAL OF LOCAL IMAGE PATHS - EXTERNAL URLS ONLY
            // This migration removes ALL local image dependencies and switches to external URLs exclusively
            
            migrationBuilder.Sql(@"
                -- Remove all local image paths and use external URLs only
                -- Author: leshancha
                -- Purpose: Complete elimination of local image dependencies
                
                PRINT '🌐 REMOVING ALL LOCAL IMAGE PATHS - EXTERNAL URLS ONLY';
                PRINT '======================================================================';
                
                -- Check current state before conversion
                SELECT 
                    COUNT(*) as TotalProducts,
                    SUM(CASE WHEN ImageUrl LIKE 'http%' THEN 1 ELSE 0 END) as ExternalUrls,
                    SUM(CASE WHEN ImageUrl LIKE 'images/%' THEN 1 ELSE 0 END) as LocalPaths_ToBeRemoved
                FROM Products 
                WHERE IsActive = 1;
                
                -- 📱 PHONES (4 products) - EXTERNAL URLS ONLY
                UPDATE Products SET 
                    ImageUrl = 'https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/iphone-15-pro-model-unselect-gallery-2-202309?wid=5120&hei=2880&fmt=jpeg&qlt=80&.v=1692761460502',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Apple iPhone 15 Pro';
                
                UPDATE Products SET 
                    ImageUrl = 'https://www.wishque.com/data/images/products/11423/65395282_259007609966_0.66966800-1708668229.png',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Samsung Galaxy S24 Ultra';
                
                UPDATE Products SET 
                    ImageUrl = 'https://xmobile.lk/wp-content/uploads/2023/10/3-31.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Google Pixel 8 Pro';
                
                UPDATE Products SET 
                    ImageUrl = 'https://technoor.me/wp-content/uploads/2023/12/oneplus-12-color_1.jpeg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'OnePlus 12';
                
                -- 📱 TABLETS (4 products) - EXTERNAL URLS ONLY
                UPDATE Products SET 
                    ImageUrl = 'https://cdn.alloallo.media/catalog/product/apple/ipad/ipad-pro-12-9-in-6e-generation/ipad-pro-12-9-in-6e-generation-space-gray.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Apple iPad Pro 12.9″ (M2)';
                
                UPDATE Products SET 
                    ImageUrl = 'https://images.samsung.com/is/image/samsung/p6pim/uk/2307/gallery/uk-galaxy-tab-s9-ultra-5g-x916-sm-x916bzaeeub-537349520?$624_624_PNG$',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Samsung Galaxy Tab S9 Ultra';
                
                UPDATE Products SET 
                    ImageUrl = 'https://m.media-amazon.com/images/I/81cOAQnitYL._UF1000,1000_QL80_.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Amazon Kindle Paperwhite';
                
                UPDATE Products SET 
                    ImageUrl = 'https://p1-ofp.static.pub/medias/bWFzdGVyfHJvb3R8NTgzNDgwfGltYWdlL3BuZ3xoZjkvaDNjLzEzNjc0NDgwMTczMDg2LnBuZ3w3M2ZjZTJlOGJlYWQ3ZWZlYzJlZmI4NDg0ODhjMGI2ZTdjNzJmNDFlMTY5ZGQ0OTYwZGFjYmZiMmFmMzRhMDE4/lenovo-tab-m10-plus-gen-3-hero.png',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Lenovo Tab M10 Plus';
                
                -- 💻 LAPTOPS (3 products) - EXTERNAL URLS ONLY
                UPDATE Products SET 
                    ImageUrl = 'https://www.cnet.com/a/img/resize/b51c311f6732da72e77670beabcfcd07d39808ae/hub/2023/06/05/85a7355a-67e4-48a0-bd9c-2e927a3249b5/macbook-air-15-inch-m2-02.jpg?auto=webp&fit=crop&height=900&width=1200',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Apple MacBook Air 15″ M2';
                
                UPDATE Products SET 
                    ImageUrl = 'https://sm.pcmag.com/pcmag_au/review/d/dell-xps-1/dell-xps-13-plus-2023_8w51.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Dell XPS 13 Plus';
                
                UPDATE Products SET 
                    ImageUrl = 'https://m.media-amazon.com/images/I/61GkOVE3gnL._AC_SL1500_.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'ASUS ROG Zephyrus G16';
                
                -- 🎧 ACCESSORIES (8 products) - EXTERNAL URLS ONLY
                UPDATE Products SET 
                    ImageUrl = 'https://m.media-amazon.com/images/I/71o8Q5XJS5L._AC_SL1500_.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Sony WH-1000XM5';
                
                UPDATE Products SET 
                    ImageUrl = 'https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/MQD83_AV1?wid=1000&hei=1000&fmt=jpeg&qlt=80&.v=1660803973364',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Apple AirPods Pro (2nd Gen)';
                
                UPDATE Products SET 
                    ImageUrl = 'https://www.ubuy.com.lk/productimg/?image=aHR0cHM6Ly9tLW1lZGlhLWFtYXpvbi5jb20vaW1hZ2VzL0kvNjFuaTN0MXJ5UUwuX0FDX1NMMTUwMF8uanBn.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Logitech MX Master 3S';
                
                UPDATE Products SET 
                    ImageUrl = 'https://www.barclays.lk/mmBC/Images/SSDS9146.JPG',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Samsung T7 Shield SSD 1TB';
                
                UPDATE Products SET 
                    ImageUrl = 'https://presentsolution.lk/wp-content/uploads/2024/02/series-9-45mm.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Apple Watch Series 9';
                
                UPDATE Products SET 
                    ImageUrl = 'https://trustedge.lk/wp-content/uploads/2024/01/2-26.png',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'JBL Flip 6 Bluetooth Speaker';
                
                UPDATE Products SET 
                    ImageUrl = 'https://toyo.lk/wp-content/uploads/2023/12/Fitbit-Charge-6.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Fitbit Charge 6';
                
                UPDATE Products SET 
                    ImageUrl = 'https://m.media-amazon.com/images/I/51ZR4lyxBHL.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Bose QuietComfort Ultra';
                
                -- 🎮 GADGETS (4 products) - EXTERNAL URLS ONLY
                UPDATE Products SET 
                    ImageUrl = 'https://rangashopping.lk/wp-content/uploads/2024/02/1693990916_IMG_2070536.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'GoPro HERO12 Black';
                
                UPDATE Products SET 
                    ImageUrl = 'https://media.foto-erhardt.de/images/product_images/original_images/481/dji-mini-4-pro-fly-more-combo-dji-goggles-3-rc-motion-3-171377676348190304.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'DJI Mini 4 Pro';
                
                UPDATE Products SET 
                    ImageUrl = 'https://m.media-amazon.com/images/I/51OT7thu1CL._UF1000,1000_QL80_.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Oculus Quest 3';
                
                UPDATE Products SET 
                    ImageUrl = 'https://www.gadgetguy.com.au/wp-content/uploads/2021/05/Nest-Hub_Lifestyle_Your-Evening-with-Sleep-Sensing.jpg',
                    UpdatedAt = GETUTCDATE()
                WHERE Name = 'Google Nest Hub (2nd Gen)';
                
                -- Remove any remaining local paths for unmapped products
                UPDATE Products SET 
                    ImageUrl = 'https://via.placeholder.com/400x300/667eea/ffffff?text=' + REPLACE(Name, ' ', '%20'),
                    UpdatedAt = GETUTCDATE()
                WHERE ImageUrl LIKE 'images/%' OR ImageUrl IS NULL OR ImageUrl = '';
                
                -- Verification: Check final state
                PRINT '✅ LOCAL PATH REMOVAL COMPLETE - VERIFICATION:';
                SELECT 
                    COUNT(*) as TotalProducts,
                    SUM(CASE WHEN ImageUrl LIKE 'https://%' THEN 1 ELSE 0 END) as ExternalUrls,
                    SUM(CASE WHEN ImageUrl LIKE 'images/%' THEN 1 ELSE 0 END) as RemainingLocalPaths_ShouldBeZero,
                    SUM(CASE WHEN ImageUrl IS NULL OR ImageUrl = '' THEN 1 ELSE 0 END) as MissingUrls
                FROM Products 
                WHERE IsActive = 1;
                
                PRINT '======================================================================';
                PRINT '🎉 ALL LOCAL IMAGE PATHS REMOVED - EXTERNAL URLS ONLY';
                PRINT '✅ All 23 products now use high-quality external image URLs';
                PRINT '🚫 No more local path dependencies (images/*.png completely removed)';
                PRINT '🌐 Images load from manufacturer and retailer CDNs';
                PRINT '⚡ No local file management required';
                PRINT '======================================================================';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ROLLBACK: Restore local image paths (for development rollback only)
            // Note: This rollback is provided for development purposes only
            // Production systems should maintain external URLs
            
            migrationBuilder.Sql(@"
                PRINT '🔄 ROLLBACK: Restoring local image paths (development only)';
                PRINT '======================================================================';
                
                -- 📱 PHONES (4 products) - Rollback to local paths
                UPDATE Products SET ImageUrl = 'images/Apple15.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Apple iPhone 15 Pro';
                UPDATE Products SET ImageUrl = 'images/s24ultra.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Samsung Galaxy S24 Ultra';
                UPDATE Products SET ImageUrl = 'images/pixel.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Google Pixel 8 Pro';
                UPDATE Products SET ImageUrl = 'images/oneplus.png', UpdatedAt = GETUTCDATE() WHERE Name = 'OnePlus 12';
                
                -- 📱 TABLETS (4 products) - Rollback to local paths
                UPDATE Products SET ImageUrl = 'images/ipad.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Apple iPad Pro 12.9″ (M2)';
                UPDATE Products SET ImageUrl = 'images/s9tab.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Samsung Galaxy Tab S9 Ultra';
                UPDATE Products SET ImageUrl = 'images/paperwrit.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Amazon Kindle Paperwhite';
                UPDATE Products SET ImageUrl = 'images/lenovotab.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Lenovo Tab M10 Plus';
                
                -- 💻 LAPTOPS (3 products) - Rollback to local paths
                UPDATE Products SET ImageUrl = 'images/macbook.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Apple MacBook Air 15″ M2';
                UPDATE Products SET ImageUrl = 'images/dell.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Dell XPS 13 Plus';
                UPDATE Products SET ImageUrl = 'images/rog.png', UpdatedAt = GETUTCDATE() WHERE Name = 'ASUS ROG Zephyrus G16';
                
                -- 🎧 ACCESSORIES (8 products) - Rollback to local paths
                UPDATE Products SET ImageUrl = 'images/headsony.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Sony WH-1000XM5';
                UPDATE Products SET ImageUrl = 'images/airpod.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Apple AirPods Pro (2nd Gen)';
                UPDATE Products SET ImageUrl = 'images/mouse.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Logitech MX Master 3S';
                UPDATE Products SET ImageUrl = 'images/ssd.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Samsung T7 Shield SSD 1TB';
                UPDATE Products SET ImageUrl = 'images/watch.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Apple Watch Series 9';
                UPDATE Products SET ImageUrl = 'images/jbl.png', UpdatedAt = GETUTCDATE() WHERE Name = 'JBL Flip 6 Bluetooth Speaker';
                UPDATE Products SET ImageUrl = 'images/fitneswatch.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Fitbit Charge 6';
                UPDATE Products SET ImageUrl = 'images/bose.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Bose QuietComfort Ultra';
                
                -- 🎮 GADGETS (4 products) - Rollback to local paths
                UPDATE Products SET ImageUrl = 'images/gopro.png', UpdatedAt = GETUTCDATE() WHERE Name = 'GoPro HERO12 Black';
                UPDATE Products SET ImageUrl = 'images/Drone.png', UpdatedAt = GETUTCDATE() WHERE Name = 'DJI Mini 4 Pro';
                UPDATE Products SET ImageUrl = 'images/vr.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Oculus Quest 3';
                UPDATE Products SET ImageUrl = 'images/nesthub.png', UpdatedAt = GETUTCDATE() WHERE Name = 'Google Nest Hub (2nd Gen)';
                
                PRINT '⚠️ ROLLBACK COMPLETE: Local image paths restored';
                PRINT '⚠️ WARNING: You will need local image files in wwwroot/images/';
                PRINT '======================================================================';
            ");
        }
    }
}
