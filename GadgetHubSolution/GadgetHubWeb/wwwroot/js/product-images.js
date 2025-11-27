// Enhanced Image Loading System for GadgetHub Products - SILENT MODE
class ProductImageManager {
    constructor() {
        // Local fallback system - no external URLs
        this.localImagePaths = {
            'Apple iPhone 15 Pro': '/images/Apple15.png',
            'Samsung Galaxy S24 Ultra': '/images/s24ultra.png',
            'Google Pixel 8 Pro': '/images/pixel.png',
            'OnePlus 12': '/images/oneplus.png',
            'Apple iPad Pro 12.9? (M2)': '/images/ipad.png',
            'Samsung Galaxy Tab S9 Ultra': '/images/s9tab.png',
            'Amazon Kindle Paperwhite': '/images/paperwrit.png',
            'Lenovo Tab M10 Plus': '/images/lenovotab.png',
            'Apple MacBook Air 15? M2': '/images/macbook.png',
            'Dell XPS 13 Plus': '/images/dell.png',
            'ASUS ROG Zephyrus G16': '/images/rog.png',
            'Sony WH-1000XM5': '/images/headsony.png',
            'Apple AirPods Pro (2nd Gen)': '/images/airpod.png',
            'Logitech MX Master 3S': '/images/mouse.png',
            'Samsung T7 Shield SSD 1TB': '/images/ssd.png',
            'Apple Watch Series 9': '/images/watch.png',
            'JBL Flip 6 Bluetooth Speaker': '/images/jbl.png',
            'Fitbit Charge 6': '/images/fitneswatch.png',
            'Bose QuietComfort Ultra': '/images/bose.png',
            'GoPro HERO12 Black': '/images/gopro.png',
            'DJI Mini 4 Pro': '/images/Drone.png',
            'Oculus Quest 3': '/images/vr.png',
            'Google Nest Hub (2nd Gen)': '/images/nesthub.png'
        };
        
        this.brandIcons = {
            'Apple': 'fab fa-apple',
            'Samsung': 'fas fa-mobile-alt',
            'Google': 'fab fa-google',
            'Microsoft': 'fab fa-microsoft',
            'ASUS': 'fas fa-laptop',
            'Dell': 'fas fa-desktop',
            'Sony': 'fas fa-headphones',
            'HP': 'fas fa-laptop',
            'NVIDIA': 'fas fa-microchip',
            'AMD': 'fas fa-microchip',
            'JBL': 'fas fa-volume-up',
            'Bose': 'fas fa-headphones',
            'Fitbit': 'fas fa-heartbeat',
            'GoPro': 'fas fa-camera',
            'DJI': 'fas fa-helicopter',
            'OnePlus': 'fas fa-mobile-alt',
            'Logitech': 'fas fa-mouse',
            'Meta': 'fas fa-vr-cardboard',
            'Oculus': 'fas fa-vr-cardboard',
            'default': 'fas fa-box'
        };
        
        this.brandColors = {
            'Apple': '#1a73e8',
            'Samsung': '#1565c0',
            'Google': '#4285f4',
            'Microsoft': '#00bcf2',
            'ASUS': '#ff6600',
            'Dell': '#007db8',
            'Sony': '#000000',
            'HP': '#0096d6',
            'NVIDIA': '#76b900',
            'AMD': '#ed1c24',
            'JBL': '#ff6900',
            'Bose': '#000000',
            'Fitbit': '#00b8a9',
            'GoPro': '#000000',
            'DJI': '#fc3c3c',
            'OnePlus': '#ff6600',
            'Logitech': '#00b8fc',
            'Meta': '#1877f2',
            'Oculus': '#1877f2',
            'default': '#28a745'
        };
        
        this.init();
    }
    
    init() {
        console.log('??? ProductImageManager initialized - SILENT MODE');
        this.processExistingImages();
        this.hideStatusBar(); // Always hide the status bar
    }
    
    // Hide the status bar completely
    hideStatusBar() {
        const statusDiv = document.getElementById('imageLoadingStatus');
        if (statusDiv) {
            statusDiv.style.display = 'none';
            statusDiv.remove(); // Remove it entirely
        }
    }
    
    processExistingImages() {
        const productImages = document.querySelectorAll('.product-image[src]');
        console.log(`?? Processing ${productImages.length} product images silently`);
        
        productImages.forEach(img => this.handleProductImage(img));
    }
    
    handleProductImage(img) {
        // Get product info
        const productName = img.getAttribute('data-product-name') || img.alt || 'Unknown Product';
        const brand = this.extractBrand(productName);
        
        // Setup event handlers
        img.onload = () => this.handleImageLoad(img, productName);
        img.onerror = () => this.handleImageError(img, brand, productName);
        
        // Check if image is already loaded
        if (img.complete) {
            if (img.naturalWidth > 0) {
                this.handleImageLoad(img, productName);
            } else {
                this.handleImageError(img, brand, productName);
            }
        }
    }
    
    handleImageLoad(img, productName) {
        console.log(`? Image loaded: ${productName}`);
        img.classList.remove('loading');
        img.classList.add('loaded');
        
        // Hide any existing placeholder
        const placeholder = this.findPlaceholder(img);
        if (placeholder) {
            placeholder.style.display = 'none';
        }
    }
    
    handleImageError(img, brand, productName) {
        console.warn(`? Image failed to load: ${productName} - ${img.src}`);
        
        // Try to use a local fallback image first
        if (this.localImagePaths[productName] && img.src !== location.origin + this.localImagePaths[productName]) {
            console.log(`?? Trying local fallback for: ${productName}`);
            img.src = this.localImagePaths[productName];
            return;
        }
        
        // If local fallback also fails, show enhanced placeholder
        img.style.display = 'none';
        this.showEnhancedPlaceholder(img, brand, productName);
    }
    
    showEnhancedPlaceholder(img, brand, productName) {
        const container = img.closest('.product-image-container');
        let placeholder = this.findPlaceholder(img);
        
        if (!placeholder) {
            placeholder = this.createPlaceholder(brand, productName);
            container.appendChild(placeholder);
        }
        
        this.updatePlaceholder(placeholder, brand, productName);
        placeholder.style.display = 'flex';
    }
    
    findPlaceholder(img) {
        const container = img.closest('.product-image-container');
        return container ? container.querySelector('.product-image-placeholder') : null;
    }
    
    createPlaceholder(brand, productName) {
        const placeholder = document.createElement('div');
        placeholder.className = 'product-image-placeholder';
        placeholder.style.cssText = `
            height: 200px;
            background: linear-gradient(135deg, ${this.getBrandColor(brand)} 0%, ${this.getBrandColorSecondary(brand)} 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            border-radius: 8px 8px 0 0;
            position: relative;
            overflow: hidden;
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
        `;
        
        return placeholder;
    }
    
    updatePlaceholder(placeholder, brand, productName) {
        const icon = this.brandIcons[brand] || this.brandIcons.default;
        
        placeholder.innerHTML = `
            <div class="text-center" style="z-index: 2; position: relative; padding: 20px;">
                <i class="${icon} fa-3x mb-3" style="margin-bottom: 12px;"></i>
                <div class="fw-bold" style="font-size: 0.9rem; line-height: 1.2; margin-bottom: 4px;">${this.truncateText(productName, 25)}</div>
                <small style="opacity: 0.8;">${brand}</small>
            </div>
            <div style="position: absolute; top: -50%; left: -50%; width: 200%; height: 200%; 
                       background: linear-gradient(45deg, transparent, rgba(255,255,255,0.1), transparent);
                       transform: rotate(45deg); animation: shimmer 3s infinite; z-index: 1;"></div>
        `;
    }
    
    extractBrand(productName) {
        const brandKeywords = Object.keys(this.brandColors);
        return brandKeywords.find(brand => 
            productName.toLowerCase().includes(brand.toLowerCase())
        ) || 'default';
    }
    
    getBrandColor(brand) {
        return this.brandColors[brand] || this.brandColors.default;
    }
    
    getBrandColorSecondary(brand) {
        const primary = this.getBrandColor(brand);
        // Darken the primary color for gradient effect
        const r = Math.max(0, parseInt(primary.slice(1, 3), 16) - 40);
        const g = Math.max(0, parseInt(primary.slice(3, 5), 16) - 40);
        const b = Math.max(0, parseInt(primary.slice(5, 7), 16) - 40);
        return `rgb(${r}, ${g}, ${b})`;
    }
    
    truncateText(text, maxLength) {
        return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
    }
    
    // Silent retry method (no status updates)
    retryFailedImages() {
        const failedImages = document.querySelectorAll('.product-image.error');
        console.log(`?? Silently retrying ${failedImages.length} failed images`);
        
        failedImages.forEach(img => {
            img.classList.remove('error');
            img.style.display = 'block';
            
            // Hide placeholder
            const placeholder = this.findPlaceholder(img);
            if (placeholder) {
                placeholder.style.display = 'none';
            }
            
            // Force reload with cache busting
            const originalSrc = img.src;
            img.src = '';
            setTimeout(() => {
                const separator = originalSrc.includes('?') ? '&' : '?';
                img.src = originalSrc + separator + 't=' + Date.now();
            }, 100);
        });
    }
}

// Enhanced CSS for silent operation
const enhancedCSS = `
@keyframes shimmer {
    0% { transform: translateX(-100%) translateY(-100%) rotate(45deg); }
    100% { transform: translateX(100%) translateY(100%) rotate(45deg); }
}

.product-image {
    transition: all 0.3s ease;
}

.product-image.loading {
    opacity: 0.8;
}

.product-image.loaded {
    opacity: 1;
    filter: none;
}

.product-image.error {
    display: none !important;
}

.product-image-placeholder {
    animation: fadeIn 0.5s ease-in;
}

@keyframes fadeIn {
    from { opacity: 0; transform: scale(0.98); }
    to { opacity: 1; transform: scale(1); }
}

.product-image-placeholder:hover {
    transform: scale(1.02);
    transition: transform 0.2s ease;
}

/* Hide any status bars */
#imageLoadingStatus {
    display: none !important;
}
`;

// Inject enhanced CSS
const style = document.createElement('style');
style.textContent = enhancedCSS;
document.head.appendChild(style);

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.productImageManager = new ProductImageManager();
    });
} else {
    window.productImageManager = new ProductImageManager();
}

console.log('??? Enhanced Product Image Manager loaded - SILENT MODE (No Status Bar)');