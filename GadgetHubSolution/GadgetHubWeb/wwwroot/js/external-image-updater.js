// ? UPDATED: External image URLs only - no local paths supported
// This system now exclusively uses external URLs for all product images

class ExternalImageUpdater {
    constructor() {
        // ? UPDATED: Only external URLs - local paths completely removed
        this.imageUrls = {
            // ?? PHONES (4 products)
            'Apple iPhone 15 Pro': 'https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/iphone-15-pro-model-unselect-gallery-2-202309?wid=5120&hei=2880&fmt=jpeg&qlt=80&.v=1692761460502',
            'Samsung Galaxy S24 Ultra': 'https://www.wishque.com/data/images/products/11423/65395282_259007609966_0.66966800-1708668229.png',
            'Google Pixel 8 Pro': 'https://xmobile.lk/wp-content/uploads/2023/10/3-31.jpg',
            'OnePlus 12': 'https://technoor.me/wp-content/uploads/2023/12/oneplus-12-color_1.jpeg',
            
            // ?? TABLETS (4 products)
            'Apple iPad Pro 12.9? (M2)': 'https://cdn.alloallo.media/catalog/product/apple/ipad/ipad-pro-12-9-in-6e-generation/ipad-pro-12-9-in-6e-generation-space-gray.jpg',
            'Samsung Galaxy Tab S9 Ultra': 'https://images.samsung.com/is/image/samsung/p6pim/uk/2307/gallery/uk-galaxy-tab-s9-ultra-5g-x916-sm-x916bzaeeub-537349520?$624_624_PNG$',
            'Amazon Kindle Paperwhite': 'https://m.media-amazon.com/images/I/81cOAQnitYL._UF1000,1000_QL80_.jpg',
            'Lenovo Tab M10 Plus': 'https://p1-ofp.static.pub/medias/bWFzdGVyfHJvb3R8NTgzNDgwfGltYWdlL3BuZ3xoZjkvaDNjLzEzNjc0NDgwMTczMDg2LnBuZ3w3M2ZjZTJlOGJlYWQ3ZWZlYzJlZmI4NDg0ODhjMGI2ZTdjNzJmNDFlMTY5ZGQ0OTYwZGFjYmZiMmFmMzRhMDE4/lenovo-tab-m10-plus-gen-3-hero.png',
            
            // ?? LAPTOPS (3 products)
            'Apple MacBook Air 15? M2': 'https://www.cnet.com/a/img/resize/b51c311f6732da72e77670beabcfcd07d39808ae/hub/2023/06/05/85a7355a-67e4-48a0-bd9c-2e927a3249b5/macbook-air-15-inch-m2-02.jpg?auto=webp&fit=crop&height=900&width=1200',
            'Dell XPS 13 Plus': 'https://sm.pcmag.com/pcmag_au/review/d/dell-xps-1/dell-xps-13-plus-2023_8w51.jpg',
            'ASUS ROG Zephyrus G16': 'https://m.media-amazon.com/images/I/61GkOVE3gnL._AC_SL1500_.jpg',
            
            // ?? ACCESSORIES (8 products)
            'Sony WH-1000XM5': 'https://m.media-amazon.com/images/I/71o8Q5XJS5L._AC_SL1500_.jpg',
            'Apple AirPods Pro (2nd Gen)': 'https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/MQD83_AV1?wid=1000&hei=1000&fmt=jpeg&qlt=80&.v=1660803973364',
            'Logitech MX Master 3S': 'https://www.ubuy.com.lk/productimg/?image=aHR0cHM6Ly9tLW1lZGlhLWFtYXpvbi5jb20vaW1hZ2VzL0kvNjFuaTN0MXJ5UUwuX0FDX1NMMTUwMF8uanBn.jpg',
            'Samsung T7 Shield SSD 1TB': 'https://www.barclays.lk/mmBC/Images/SSDS9146.JPG',
            'Apple Watch Series 9': 'https://presentsolution.lk/wp-content/uploads/2024/02/series-9-45mm.jpg',
            'JBL Flip 6 Bluetooth Speaker': 'https://trustedge.lk/wp-content/uploads/2024/01/2-26.png',
            'Fitbit Charge 6': 'https://toyo.lk/wp-content/uploads/2023/12/Fitbit-Charge-6.jpg',
            'Bose QuietComfort Ultra': 'https://m.media-amazon.com/images/I/51ZR4lyxBHL.jpg',
            
            // ?? GADGETS (4 products)
            'GoPro HERO12 Black': 'https://rangashopping.lk/wp-content/uploads/2024/02/1693990916_IMG_2070536.jpg',
            'DJI Mini 4 Pro': 'https://media.foto-erhardt.de/images/product_images/original_images/481/dji-mini-4-pro-fly-more-combo-dji-goggles-3-rc-motion-3-171377676348190304.jpg',
            'Oculus Quest 3': 'https://m.media-amazon.com/images/I/51OT7thu1CL._UF1000,1000_QL80_.jpg',
            'Google Nest Hub (2nd Gen)': 'https://www.gadgetguy.com.au/wp-content/uploads/2021/05/Nest-Hub_Lifestyle_Your-Evening-with-Sleep-Sensing.jpg'
        };
        
        this.init();
    }
    
    init() {
        console.log('?? External Image Updater initialized - EXTERNAL URLS ONLY');
        console.log('?? Local image paths are no longer supported');
        console.log('?? All images load from high-quality external sources');
        this.updateAllProductImages();
    }
    
    async updateAllProductImages() {
        console.log('?? Starting external image URL update (local paths removed)...');
        
        // Find all product images on the page
        const productImages = document.querySelectorAll('.product-image');
        console.log(`Found ${productImages.length} product images to update`);
        
        let updatedCount = 0;
        
        productImages.forEach(img => {
            const productName = img.alt || img.getAttribute('data-product-name') || '';
            
            if (this.imageUrls[productName]) {
                console.log(`?? Updating image for: ${productName}`);
                
                // Set the external URL (no local fallback)
                const newUrl = this.imageUrls[productName];
                
                // Add loading state
                img.style.opacity = '0.7';
                img.style.filter = 'blur(2px)';
                
                // Update the src with external URL
                img.src = newUrl;
                
                // Handle successful load
                img.onload = () => {
                    img.style.opacity = '1';
                    img.style.filter = 'none';
                    console.log(`? Successfully loaded external image: ${productName}`);
                    
                    // Hide any placeholder
                    const placeholder = this.findPlaceholder(img);
                    if (placeholder) {
                        placeholder.style.display = 'none';
                    }
                };
                
                // Handle load errors with external fallback only
                img.onerror = () => {
                    console.warn(`? Failed to load external image for: ${productName}`);
                    this.handleImageError(img, productName);
                };
                
                updatedCount++;
            } else {
                // For unknown products, use external placeholder
                const fallbackUrl = `https://via.placeholder.com/400x300/667eea/ffffff?text=${encodeURIComponent(productName)}`;
                img.src = fallbackUrl;
                console.log(`?? Using external placeholder for: ${productName}`);
            }
        });
        
        console.log(`?? Updated ${updatedCount} product images with external URLs`);
        console.log(`?? All images now load from external sources - no local dependencies`);
        
        // Also update any product cards that might load later
        this.setupDynamicImageUpdates();
    }
    
    setupDynamicImageUpdates() {
        // Watch for new product cards being added to the page
        if ('MutationObserver' in window) {
            const observer = new MutationObserver(mutations => {
                mutations.forEach(mutation => {
                    mutation.addedNodes.forEach(node => {
                        if (node.nodeType === 1) {
                            const newImages = node.querySelectorAll ? node.querySelectorAll('.product-image') : [];
                            newImages.forEach(img => {
                                const productName = img.alt || img.getAttribute('data-product-name') || '';
                                if (this.imageUrls[productName]) {
                                    img.src = this.imageUrls[productName];
                                } else {
                                    // External placeholder for unknown products
                                    img.src = `https://via.placeholder.com/400x300/667eea/ffffff?text=${encodeURIComponent(productName)}`;
                                }
                            });
                        }
                    });
                });
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        }
    }
    
    findPlaceholder(img) {
        // Look for associated placeholder
        const container = img.closest('.product-image-container') || img.parentElement;
        return container ? container.querySelector('.product-image-placeholder') : null;
    }
    
    handleImageError(img, productName) {
        // ? UPDATED: Only external fallback - no local path fallback
        const fallbackUrl = `https://via.placeholder.com/400x300/667eea/ffffff?text=${encodeURIComponent(productName)}`;
        
        // Try external placeholder
        const fallbackImg = new Image();
        fallbackImg.onload = () => {
            img.src = fallbackUrl;
            img.style.opacity = '1';
            img.style.filter = 'none';
            console.log(`?? Using external placeholder for: ${productName}`);
        };
        fallbackImg.onerror = () => {
            // If external placeholder also fails, hide image
            img.style.display = 'none';
            const placeholder = this.findPlaceholder(img);
            if (placeholder) {
                placeholder.style.display = 'flex';
                placeholder.innerHTML = `<div style="color: #666; font-size: 12px; text-align: center;">Image unavailable<br/>${productName}</div>`;
            }
            console.warn(`?? All external image sources failed for: ${productName}`);
        };
        fallbackImg.src = fallbackUrl;
    }
}

// Auto-initialize when page loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        new ExternalImageUpdater();
    });
} else {
    new ExternalImageUpdater();
}

// Export for manual use
window.ExternalImageUpdater = ExternalImageUpdater;

console.log('?? External Image URL system loaded - EXTERNAL URLS ONLY (no local paths)');
console.log('? All 23 products use high-quality external images');
console.log('?? Local image dependencies completely removed');