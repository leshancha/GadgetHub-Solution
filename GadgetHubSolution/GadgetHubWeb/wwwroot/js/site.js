/**
 * Gadget Hub Site JavaScript
 * Built by leshancha at 2025-07-31 10:33:01 UTC
 */

// Global GadgetHub namespace
window.GadgetHub = {
    // Initialize the application
    init: function () {
        console.log('🚀 Gadget Hub initialized by leshancha at 2025-07-31 10:33:01 UTC');
        this.setupSearch();
        this.setupCart();
        this.setupNavigation();
        this.setupNotifications();
        this.setupProductDetailEnhancements();
        this.setupLazyLoading();
        this.setupThemeToggle();
        // ✅ NEW: Enhanced image loading system with URL switching
        this.setupImageUrlSwitching();
        this.setupImageLoading(); // ✅ Add enhanced image loading
    },

    // ✅ NEW: Enhanced Image Loading System
    setupImageLoading: function() {
        // Set up global image error handlers
        this.setupImageErrorHandlers();
        this.setupImagePreloading();
        this.setupImageFallbacks();
        
        console.log('🖼️ Enhanced image loading system initialized');
    },

    // ✅ NEW: Set up image error handlers for all images
    setupImageErrorHandlers: function() {
        // Handle images that fail to load
        document.addEventListener('error', (e) => {
            if (e.target.tagName === 'IMG') {
                this.handleImageError(e.target);
            }
        }, true);

        // Set up observers for product images
        const productImages = document.querySelectorAll('.product-image, .main-product-image');
        productImages.forEach(img => {
            this.enhanceProductImage(img);
        });
    },

    // ✅ NEW: Handle individual image errors
    handleImageError: function(img) {
        console.warn(`❌ Image failed to load: ${img.src}`);
        
        // Hide the failed image
        img.style.display = 'none';
        
        // Show the placeholder if it exists
        const placeholder = this.findImagePlaceholder(img);
        if (placeholder) {
            placeholder.style.display = 'flex';
            placeholder.classList.remove('d-none');
        }
        
        // Add error class for styling
        img.classList.add('image-error');
        
        // Try alternative image sources if available
        this.tryAlternativeImage(img);
    },

    // ✅ NEW: Find associated placeholder for an image
    findImagePlaceholder: function(img) {
        // Look for sibling placeholder
        let placeholder = img.nextElementSibling;
        if (placeholder && placeholder.classList.contains('product-image-placeholder')) {
            return placeholder;
        }
        
        // Look for placeholder in parent container
        const container = img.closest('.product-image-container');
        if (container) {
            placeholder = container.querySelector('.product-image-placeholder, .product-detail-placeholder');
            if (placeholder) return placeholder;
        }
        
        return null;
    },

    // ✅ NEW: Try alternative image sources
    tryAlternativeImage: function(img) {
        // List of fallback image services with better variety
        const fallbackServices = [
            'https://via.placeholder.com/400x300/667eea/ffffff?text=',
            'https://dummyimage.com/400x300/667eea/ffffff&text=',
            'https://fakeimg.pl/400x300/667eea/ffffff/?text=',
        ];
        
        const productName = img.alt || 'Product';
        const encodedName = encodeURIComponent(productName.replace(/[^\w\s]/gi, '').replace(/\s+/g, '+'));
        
        // Try fallback services in order
        const currentFallbackIndex = parseInt(img.getAttribute('data-fallback-index') || '0');
        
        if (currentFallbackIndex < fallbackServices.length && !img.hasAttribute('data-all-fallbacks-tried')) {
            img.setAttribute('data-fallback-index', (currentFallbackIndex + 1).toString());
            
            // Mark as tried all fallbacks if this is the last one
            if (currentFallbackIndex === fallbackServices.length - 1) {
                img.setAttribute('data-all-fallbacks-tried', 'true');
            }
            
            const newSrc = fallbackServices[currentFallbackIndex] + encodedName;
            img.src = newSrc;
            console.log(`🔄 Trying fallback ${currentFallbackIndex + 1}/${fallbackServices.length} for: ${productName}`);
            console.log(`🔗 URL: ${newSrc}`);
        } else {
            console.warn(`⚠️ All fallback services exhausted for: ${productName}`);
            // At this point, the placeholder should be shown instead
        }
    },

    // ✅ NEW: Enhance product images with loading states
    enhanceProductImage: function(img) {
        if (!img) return;
        
        // Add loading state
        img.style.opacity = '0.7';
        img.style.filter = 'blur(2px)';
        
        // Handle successful load
        img.addEventListener('load', () => {
            img.style.opacity = '1';
            img.style.filter = 'none';
            img.style.display = 'block';
            
            // Hide placeholder on successful load
            const placeholder = this.findImagePlaceholder(img);
            if (placeholder) {
                placeholder.style.display = 'none';
                placeholder.classList.add('d-none');
            }
            
            console.log(`✅ Image loaded successfully: ${img.alt || img.src}`);
        });
        
        // Handle load errors
        img.addEventListener('error', () => {
            this.handleImageError(img);
        });
    },

    // ✅ NEW: Set up image preloading for better performance
    setupImagePreloading: function() {
        // Preload images that are likely to be viewed
        const productCards = document.querySelectorAll('.product-card');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target.querySelector('.product-image, .main-product-image');
                        if (img && img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                        }
                        imageObserver.unobserve(entry.target);
                    }
                });
            }, {
                rootMargin: '50px'
            });

            productCards.forEach(card => {
                imageObserver.observe(card);
            });
        }
    },

    // ✅ NEW: Set up fallback mechanisms
    setupImageFallbacks: function() {
        // Create CSS for better image placeholders
        const imageCSS = `
            .product-image-placeholder {
                transition: all 0.3s ease;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                position: relative;
                overflow: hidden;
            }
            
            .product-image-placeholder::before {
                content: '';
                position: absolute;
                top: -50%;
                left: -50%;
                width: 200%;
                height: 200%;
                background: linear-gradient(45deg, transparent, rgba(255,255,255,0.1), transparent);
                transform: rotate(45deg);
                animation: shimmer 2s infinite;
            }
            
            @keyframes shimmer {
                0% { transform: translateX(-100%) translateY(-100%) rotate(45deg); }
                100% { transform: translateX(100%) translateY(100%) rotate(45deg); }
            }
            
            .image-error {
                display: none !important;
            }
            
            .product-image {
                transition: all 0.3s ease;
            }
            
            .product-image:hover {
                transform: scale(1.05);
            }
        `;
        
        const style = document.createElement('style');
        style.textContent = imageCSS;
        document.head.appendChild(style);
    },

    // Theme Toggle Functionality
    setupThemeToggle: function() {
        // Create theme toggle button
        const themeToggle = document.createElement('button');
        themeToggle.className = 'theme-toggle';
        themeToggle.setAttribute('aria-label', 'Toggle dark/light theme');
        themeToggle.innerHTML = '<i class="fas fa-moon"></i>';
        
        // Load saved theme or detect system preference
        const savedTheme = localStorage.getItem('gadgethub-theme');
        const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const currentTheme = savedTheme || (systemPrefersDark ? 'dark' : 'light');
        
        // Apply initial theme
        this.setTheme(currentTheme);
        this.updateThemeToggleIcon(themeToggle, currentTheme);
        
        // Add click event
        themeToggle.addEventListener('click', () => {
            const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
            this.setTheme(newTheme);
            this.updateThemeToggleIcon(themeToggle, newTheme);
            localStorage.setItem('gadgethub-theme', newTheme);
            
            // Add ripple effect
            this.createRippleEffect(event, themeToggle);
            
            // Show notification
            this.showNotification(
                `Switched to ${newTheme} theme`, 
                'info', 
                2000
            );
        });
        
        // Add to DOM
        document.body.appendChild(themeToggle);
        
        // Listen for system theme changes
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            if (!localStorage.getItem('gadgethub-theme')) {
                const newTheme = e.matches ? 'dark' : 'light';
                this.setTheme(newTheme);
                this.updateThemeToggleIcon(themeToggle, newTheme);
            }
        });
    },

    setTheme: function(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        
        // Update meta theme-color for mobile browsers
        let metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (!metaThemeColor) {
            metaThemeColor = document.createElement('meta');
            metaThemeColor.name = 'theme-color';
            document.head.appendChild(metaThemeColor);
        }
        
        metaThemeColor.content = theme === 'dark' ? '#0f172a' : '#10b981';
        
        // Add smooth transition class
        document.body.classList.add('theme-transitioning');
        setTimeout(() => {
            document.body.classList.remove('theme-transitioning');
        }, 300);
    },

    updateThemeToggleIcon: function(toggle, theme) {
        const icon = toggle.querySelector('i');
        if (theme === 'dark') {
            icon.className = 'fas fa-sun';
            toggle.setAttribute('aria-label', 'Switch to light theme');
        } else {
            icon.className = 'fas fa-moon';
            toggle.setAttribute('aria-label', 'Switch to dark theme');
        }
    },

    // Enhanced Product Detail Page functionality
    setupProductDetailEnhancements: function() {
        if (document.querySelector('.product-detail-page')) {
            this.setupImageGallery();
            this.setupProductInteractions();
            this.setupQuantityControls();
        }
    },

    // Enhanced Image Gallery
    setupImageGallery: function() {
        const mainImage = document.querySelector('.main-product-image');
        const thumbnails = document.querySelectorAll('.thumbnail');
        
        if (mainImage && thumbnails.length > 0) {
            // Add zoom functionality
            this.addImageZoom(mainImage);
            
            // Enhanced thumbnail interactions
            thumbnails.forEach((thumbnail, index) => {
                thumbnail.addEventListener('click', (e) => {
                    this.switchMainImage(e.target.src, thumbnails);
                    this.createRippleEffect(e, thumbnail);
                });
                
                // Add loading animation
                thumbnail.addEventListener('load', () => {
                    thumbnail.style.opacity = '1';
                });
            });
        }
    },

    // Add zoom functionality to main image
    addImageZoom: function(image) {
        let isZoomed = false;
        
        image.addEventListener('click', () => {
            if (!isZoomed) {
                this.showImageModal(image.src, image.alt);
            }
        });
        
        // Add zoom cursor
        image.style.cursor = 'zoom-in';
    },

    // Show image in modal
    showImageModal: function(src, alt) {
        const modal = document.createElement('div');
        modal.className = 'image-modal';
        modal.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.9);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 2000;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        
        const img = document.createElement('img');
        img.src = src;
        img.alt = alt;
        img.style.cssText = `
            max-width: 90%;
            max-height: 90%;
            object-fit: contain;
            border-radius: 12px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.5);
            transform: scale(0.8);
            transition: transform 0.3s ease;
        `;
        
        const closeBtn = document.createElement('button');
        closeBtn.innerHTML = '×';
        closeBtn.style.cssText = `
            position: absolute;
            top: 20px;
            right: 30px;
            background: none;
            border: none;
            color: white;
            font-size: 3rem;
            cursor: pointer;
            z-index: 2001;
            transition: transform 0.2s ease;
        `;
        
        closeBtn.addEventListener('click', () => this.closeImageModal(modal));
        closeBtn.addEventListener('mouseenter', () => closeBtn.style.transform = 'scale(1.1)');
        closeBtn.addEventListener('mouseleave', () => closeBtn.style.transform = 'scale(1)');
        
        modal.appendChild(img);
        modal.appendChild(closeBtn);
        document.body.appendChild(modal);
        
        // Animate in
        setTimeout(() => {
            modal.style.opacity = '1';
            img.style.transform = 'scale(1)';
        }, 10);
        
        // Close on background click
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                this.closeImageModal(modal);
            }
        });
        
        // Close on escape key
        const escapeHandler = (e) => {
            if (e.key === 'Escape') {
                this.closeImageModal(modal);
                document.removeEventListener('keydown', escapeHandler);
            }
        };
        document.addEventListener('keydown', escapeHandler);
    },

    // Close image modal
    closeImageModal: function(modal) {
        modal.style.opacity = '0';
        modal.querySelector('img').style.transform = 'scale(0.8)';
        setTimeout(() => {
            if (modal.parentElement) {
                modal.remove();
            }
        }, 300);
    },

    // Switch main image with animation
    switchMainImage: function(newSrc, thumbnails) {
        const mainImage = document.querySelector('.main-product-image');
        const placeholder = document.querySelector('.product-detail-placeholder');
        
        if (mainImage && newSrc) {
            // Add loading state
            mainImage.style.opacity = '0.5';
            
            // Create new image to preload
            const newImage = new Image();
            newImage.onload = () => {
                mainImage.src = newSrc;
                mainImage.style.opacity = '1';
                mainImage.style.display = 'block';
                if (placeholder) placeholder.style.display = 'none';
            };
            newImage.onerror = () => {
                mainImage.style.opacity = '1';
                mainImage.style.display = 'none';
                if (placeholder) placeholder.style.display = 'flex';
            };
            newImage.src = newSrc;
            
            // Update active thumbnail
            thumbnails.forEach(thumb => {
                thumb.classList.remove('active');
                thumb.style.borderColor = '#e2e8f0';
                thumb.style.transform = 'scale(1)';
            });
            
            // Find and activate current thumbnail
            const activeThumbnail = Array.from(thumbnails).find(thumb => thumb.src === newSrc);
            if (activeThumbnail) {
                activeThumbnail.classList.add('active');
                activeThumbnail.style.borderColor = '#10b981';
                activeThumbnail.style.transform = 'scale(1.05)';
            }
        }
    },

    // Enhanced Product Interactions
    setupProductInteractions: function() {
        // Enhanced Add to Cart
        const addToCartBtns = document.querySelectorAll('[onclick*="addToCart"]');
        addToCartBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.createRippleEffect(e, btn);
            });
        });
        
        // Enhanced Wishlist
        const wishlistBtns = document.querySelectorAll('.btn-outline-warning');
        wishlistBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.toggleWishlist(e, btn);
            });
        });
    },

    // Enhanced Quantity Controls
    setupQuantityControls: function() {
        const quantityInput = document.getElementById('quantity');
        const minusBtn = document.querySelector('[onclick*="changeQuantity(-1)"]');
        const plusBtn = document.querySelector('[onclick*="changeQuantity(1)"]');
        
        if (quantityInput && minusBtn && plusBtn) {
            // Enhanced change quantity function
            window.changeQuantity = (change) => {
                const currentValue = parseInt(quantityInput.value);
                const newValue = currentValue + change;
                
                if (newValue >= 1 && newValue <= 100) {
                    // Animate the change
                    quantityInput.style.transform = 'scale(1.1)';
                    quantityInput.style.color = change > 0 ? '#10b981' : '#ef4444';
                    
                    setTimeout(() => {
                        quantityInput.value = newValue;
                        quantityInput.style.transform = 'scale(1)';
                        quantityInput.style.color = '';
                    }, 150);
                    
                    // Update price if available
                    this.updateTotalPrice(newValue);
                }
                
                // Button feedback
                const btnToAnimate = change > 0 ? plusBtn : minusBtn;
                btnToAnimate.style.transform = 'scale(0.95)';
                setTimeout(() => btnToAnimate.style.transform = 'scale(1)', 100);
            };
            
            // Keyboard support
            quantityInput.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    changeQuantity(1);
                } else if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    changeQuantity(-1);
                }
            });
        }
    },

    // Update total price (if applicable)
    updateTotalPrice: function(quantity) {
        const priceElement = document.querySelector('.current-price');
        if (priceElement) {
            // This would calculate total based on unit price
            // For now, just add visual feedback
            priceElement.style.transform = 'scale(1.05)';
            priceElement.style.color = '#10b981';
            setTimeout(() => {
                priceElement.style.transform = 'scale(1)';
                priceElement.style.color = '';
            }, 300);
        }
    },

    // Toggle Wishlist with animation
    toggleWishlist: function(event, btn) {
        event.preventDefault();
        
        const icon = btn.querySelector('i');
        const isActive = btn.classList.contains('active');
        
        if (!isActive) {
            // Add to wishlist animation
            icon.className = 'fas fa-heart me-2';
            btn.classList.add('active');
            btn.classList.remove('btn-outline-warning');
            btn.classList.add('btn-warning');
            
            // Heart animation
            icon.style.transform = 'scale(1.3)';
            icon.style.color = '#dc2626';
            setTimeout(() => {
                icon.style.transform = 'scale(1)';
                icon.style.color = '';
            }, 300);
            
            this.showNotification('Added to wishlist!', 'success');
        } else {
            // Remove from wishlist
            icon.className = 'far fa-heart me-2';
            btn.classList.remove('active');
            btn.classList.add('btn-outline-warning');
            btn.classList.remove('btn-warning');
            
            this.showNotification('Removed from wishlist', 'info');
        }
        
        this.createRippleEffect(event, btn);
    },

    // Enhanced Search functionality
    setupSearch: function () {
        const searchInput = document.querySelector('input[name="SearchQuery"], input[placeholder*="Search"]');

        if (searchInput) {
            let searchTimeout;

            searchInput.addEventListener('input', function () {
                const query = this.value.trim();

                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    if (query.length >= 2) {
                        GadgetHub.showSearchSuggestions(searchInput, query);
                    } else {
                        GadgetHub.hideSearchSuggestions();
                    }
                }, 300);
            });

            // Hide suggestions when clicking outside
            document.addEventListener('click', function (e) {
                if (!e.target.closest('.search-container, .search-box')) {
                    GadgetHub.hideSearchSuggestions();
                }
            });

            // Enhanced keyboard navigation
            searchInput.addEventListener('keydown', function(e) {
                const suggestions = document.querySelectorAll('.search-suggestion-item');
                const activeSuggestion = document.querySelector('.search-suggestion-item.active');
                
                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    if (activeSuggestion) {
                        activeSuggestion.classList.remove('active');
                        const next = activeSuggestion.nextElementSibling;
                        if (next) next.classList.add('active');
                        else suggestions[0]?.classList.add('active');
                    } else {
                        suggestions[0]?.classList.add('active');
                    }
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    if (activeSuggestion) {
                        activeSuggestion.classList.remove('active');
                        const prev = activeSuggestion.previousElementSibling;
                        if (prev) prev.classList.add('active');
                        else suggestions[suggestions.length - 1]?.classList.add('active');
                    } else {
                        suggestions[suggestions.length - 1]?.classList.add('active');
                    }
                } else if (e.key === 'Enter' && activeSuggestion) {
                    e.preventDefault();
                    activeSuggestion.click();
                }
            });
        }
    },

    showSearchSuggestions: function (input, query) {
        // Enhanced search suggestions with product data
        const suggestions = [
            { name: 'iPhone 15 Pro Max', category: 'Smartphones', image: '📱' },
            { name: 'Samsung Galaxy S24 Ultra', category: 'Smartphones', image: '📱' },
            { name: 'MacBook Pro M3 Max', category: 'Laptops', image: '💻' },
            { name: 'iPad Pro M4', category: 'Tablets', image: '📱' },
            { name: 'AirPods Pro', category: 'Accessories', image: '🎧' },
            { name: 'Apple Watch Ultra 2', category: 'Smart Watches', image: '⌚' }
        ].filter(item => item.name.toLowerCase().includes(query.toLowerCase()));

        if (suggestions.length > 0) {
            this.renderSearchSuggestions(input, suggestions);
        } else {
            this.hideSearchSuggestions();
        }
    },

    renderSearchSuggestions: function (input, suggestions) {
        this.hideSearchSuggestions();

        const container = document.createElement('div');
        container.className = 'search-suggestions';
        container.style.cssText = `
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: var(--card-bg);
            border: 2px solid var(--border-primary);
            border-radius: 12px;
            box-shadow: var(--shadow-lg);
            z-index: 1000;
            max-height: 300px;
            overflow-y: auto;
            border-top: none;
            margin-top: 2px;
        `;

        suggestions.forEach((suggestion, index) => {
            const item = document.createElement('div');
            item.className = 'search-suggestion-item';
            item.style.cssText = `
                padding: 1rem 1.25rem;
                cursor: pointer;
                border-bottom: 1px solid var(--border-primary);
                transition: all 0.2s ease;
                display: flex;
                align-items: center;
                gap: 0.75rem;
                color: var(--text-primary);
            `;
            
            item.innerHTML = `
                <span style="font-size: 1.5rem;">${suggestion.image}</span>
                <div>
                    <div style="font-weight: 600; color: var(--text-primary);">${suggestion.name}</div>
                    <div style="font-size: 0.875rem; color: var(--text-secondary);">${suggestion.category}</div>
                </div>
            `;

            item.addEventListener('mouseenter', function () {
                document.querySelectorAll('.search-suggestion-item').forEach(i => i.classList.remove('active'));
                this.classList.add('active');
                this.style.backgroundColor = 'var(--bg-secondary)';
                this.style.borderColor = 'var(--primary-color)';
            });

            item.addEventListener('mouseleave', function () {
                this.style.backgroundColor = '';
                this.style.borderColor = '';
            });

            item.addEventListener('click', function () {
                input.value = suggestion.name;
                input.form?.submit();
            });

            container.appendChild(item);
        });

        input.parentElement.style.position = 'relative';
        input.parentElement.appendChild(container);

        // Animate in
        setTimeout(() => {
            container.style.transform = 'translateY(0)';
            container.style.opacity = '1';
        }, 10);
    },

    hideSearchSuggestions: function () {
        const suggestions = document.querySelector('.search-suggestions');
        if (suggestions) {
            suggestions.style.opacity = '0';
            suggestions.style.transform = 'translateY(-10px)';
            setTimeout(() => suggestions.remove(), 200);
        }
    },

    // Enhanced Cart functionality
    setupCart: function () {
        this.updateCartCount();
        this.setupCartNotifications();
        this.setupCartAnimations();
    },

    setupCartAnimations: function() {
        // Animate cart icon when items are added
        const cartIcons = document.querySelectorAll('.fa-shopping-cart');
        cartIcons.forEach(icon => {
            icon.addEventListener('animationend', () => {
                icon.classList.remove('cart-bounce');
            });
        });
    },

    updateCartCount: function () {
        const cartCount = this.getCartCount();
        const cartBadges = document.querySelectorAll('.cart-count');

        cartBadges.forEach(badge => {
            badge.textContent = cartCount;
            badge.style.display = cartCount > 0 ? 'inline-block' : 'none';
            
            if (cartCount > 0) {
                badge.classList.add('cart-bounce');
            }
        });
    },

    getCartCount: function () {
        const sessionCart = sessionStorage.getItem('gadgethub_cart_count');
        return sessionCart ? parseInt(sessionCart) : 0;
    },

    addToCart: function(productId, quantity = 1) {
        const currentCount = this.getCartCount();
        const newCount = currentCount + quantity;
        
        sessionStorage.setItem('gadgethub_cart_count', newCount.toString());
        this.updateCartCount();
        
        // Animate cart icon
        const cartIcons = document.querySelectorAll('.fa-shopping-cart');
        cartIcons.forEach(icon => {
            icon.classList.add('cart-bounce');
        });
        
        this.showNotification(`Added ${quantity} item(s) to cart!`, 'success');
    },

    setupCartNotifications: function () {
        // Listen for cart update events
        document.addEventListener('cartUpdated', (e) => {
            this.updateCartCount();
            this.showNotification(e.detail.message, e.detail.type || 'success');
        });
    },

    // Enhanced Navigation
    setupNavigation: function () {
        this.setupMobileNavigation();
        this.setupScrollEffects();
        this.setupActiveNavHighlight();
    },

    setupMobileNavigation: function () {
        const navbarToggler = document.querySelector('.navbar-toggler');
        const navbarCollapse = document.querySelector('.navbar-collapse');

        if (navbarToggler && navbarCollapse) {
            navbarToggler.addEventListener('click', function () {
                navbarCollapse.classList.toggle('show');
            });
        }
    },

    setupScrollEffects: function () {
        let lastScroll = 0;
        const navbar = document.querySelector('.navbar');

        if (navbar) {
            window.addEventListener('scroll', () => {
                const currentScroll = window.pageYOffset;

                if (currentScroll > lastScroll && currentScroll > 100) {
                    navbar.style.transform = 'translateY(-100%)';
                } else {
                    navbar.style.transform = 'translateY(0)';
                }

                lastScroll = currentScroll;
            });
        }
    },

    setupActiveNavHighlight: function () {
        const navLinks = document.querySelectorAll('.nav-link');
        const currentPath = window.location.pathname;

        navLinks.forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });
    },

    // Enhanced Notifications
    setupNotifications: function () {
        this.createNotificationContainer();
    },

    createNotificationContainer: function () {
        if (!document.getElementById('notification-container')) {
            const container = document.createElement('div');
            container.id = 'notification-container';
            container.style.cssText = `
                position: fixed;
                top: 100px;
                right: 20px;
                z-index: 1060;
                max-width: 400px;
            `;
            document.body.appendChild(container);
        }
    },

    showNotification: function (message, type = 'info', duration = 5000) {
        const container = document.getElementById('notification-container');
        if (!container) return;

        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show mb-2`;
        notification.style.cssText = `
            box-shadow: var(--shadow-lg);
            border: none;
            border-radius: 12px;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            background-color: var(--card-bg);
            color: var(--text-primary);
            border: 1px solid var(--border-primary);
        `;

        const iconMap = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-triangle',
            warning: 'fa-exclamation-triangle',
            info: 'fa-info-circle'
        };

        notification.innerHTML = `
            <i class="fas ${iconMap[type]} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        container.appendChild(notification);

        // Animate in
        setTimeout(() => notification.style.transform = 'translateX(0)', 100);

        // Auto remove
        setTimeout(() => {
            if (notification.parentElement) {
                notification.style.transform = 'translateX(100%)';
                setTimeout(() => notification.remove(), 300);
            }
        }, duration);

        // Manual close
        const closeBtn = notification.querySelector('.btn-close');
        closeBtn.addEventListener('click', () => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        });
    },

    // Enhanced Lazy Loading
    setupLazyLoading: function () {
        const images = document.querySelectorAll('img[data-src]');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.remove('lazy');
                        observer.unobserve(img);
                    }
                });
            });

            images.forEach(img => imageObserver.observe(img));
        } else {
            // Fallback for older browsers
            images.forEach(img => {
                img.src = img.dataset.src;
            });
        }
    },

    // ✅ NEW: Image URL switching functionality
    setupImageUrlSwitching: function() {
        // Add image URL switching controls if they don't exist
        this.createImageUrlSwitcher();
        
        // Set up event listeners for switching
        this.setupImageUrlSwitchEvents();
        
        console.log('🔄 Image URL switching system initialized');
    },

    // ✅ NEW: Create image URL switcher controls
    createImageUrlSwitcher: function() {
        // Check if switcher already exists
        if (document.getElementById('image-url-switcher')) {
            return;
        }

        // Create switcher container
        const switcherContainer = document.createElement('div');
        switcherContainer.id = 'image-url-switcher';
        switcherContainer.style.cssText = `
            position: fixed;
            bottom: 20px;
            left: 20px;
            background: var(--card-bg);
            border: 2px solid var(--border-primary);
            border-radius: 12px;
            padding: 1rem;
            box-shadow: var(--shadow-lg);
            z-index: 1050;
            min-width: 250px;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
        `;

        switcherContainer.innerHTML = `
            <div style="margin-bottom: 0.75rem; font-weight: 600; color: var(--text-primary);">
                🖼️ Image URL Type
            </div>
            <div style="display: flex; gap: 0.5rem; margin-bottom: 0.75rem;">
                <button class="image-url-btn" data-type="local" style="
                    padding: 0.5rem 1rem;
                    border: 2px solid var(--primary-color);
                    background: var(--primary-color);
                    color: white;
                    border-radius: 8px;
                    cursor: pointer;
                    font-size: 0.875rem;
                    font-weight: 500;
                    transition: all 0.2s ease;
                ">🏠 Local</button>
                <button class="image-url-btn" data-type="external" style="
                    padding: 0.5rem 1rem;
                    border: 2px solid var(--border-primary);
                    background: transparent;
                    color: var(--text-primary);
                    border-radius: 8px;
                    cursor: pointer;
                    font-size: 0.875rem;
                    font-weight: 500;
                    transition: all 0.2s ease;
                ">🌐 External</button>
            </div>
            <div style="font-size: 0.75rem; color: var(--text-secondary); line-height: 1.4;">
                <div>🏠 Local: Self-hosted images</div>
                <div>🌐 External: CDN images</div>
            </div>
        `;

        document.body.appendChild(switcherContainer);

        // Add hover effects
        const buttons = switcherContainer.querySelectorAll('.image-url-btn');
        buttons.forEach(btn => {
            btn.addEventListener('mouseenter', () => {
                if (!btn.classList.contains('active')) {
                    btn.style.borderColor = 'var(--primary-color)';
                    btn.style.color = 'var(--primary-color)';
                }
            });
            
            btn.addEventListener('mouseleave', () => {
                if (!btn.classList.contains('active')) {
                    btn.style.borderColor = 'var(--border-primary)';
                    btn.style.color = 'var(--text-primary)';
                }
            });
        });
    },

    // ✅ NEW: Set up image URL switch events
    setupImageUrlSwitchEvents: function() {
        document.addEventListener('click', async (e) => {
            if (e.target.classList.contains('image-url-btn')) {
                e.preventDefault();
                
                const urlType = e.target.getAttribute('data-type');
                const allButtons = document.querySelectorAll('.image-url-btn');
                
                // Update button states
                allButtons.forEach(btn => {
                    if (btn.getAttribute('data-type') === urlType) {
                        btn.style.background = 'var(--primary-color)';
                        btn.style.borderColor = 'var(--primary-color)';
                        btn.style.color = 'white';
                        btn.classList.add('active');
                    } else {
                        btn.style.background = 'transparent';
                        btn.style.borderColor = 'var(--border-primary)';
                        btn.style.color = 'var(--text-primary)';
                        btn.classList.remove('active');
                    }
                });
                
                // Show loading state
                e.target.innerHTML = urlType === 'local' ? '🔄 Switching...' : '🔄 Switching...';
                e.target.disabled = true;
                
                try {
                    await this.switchImageUrls(urlType);
                    
                    // Success state
                    e.target.innerHTML = urlType === 'local' ? '✅ Local' : '✅ External';
                    
                    setTimeout(() => {
                        e.target.innerHTML = urlType === 'local' ? '🏠 Local' : '🌐 External';
                        e.target.disabled = false;
                    }, 2000);
                    
                } catch (error) {
                    // Error state
                    e.target.innerHTML = urlType === 'local' ? '❌ Local' : '❌ External';
                    
                    setTimeout(() => {
                        e.target.innerHTML = urlType === 'local' ? '🏠 Local' : '🌐 External';
                        e.target.disabled = false;
                    }, 3000);
                }
            }
        });
    },

    // ✅ NEW: Switch image URLs via API
    switchImageUrls: async function(urlType) {
        try {
            console.log(`🔄 Switching to ${urlType} image URLs...`);
            
            const response = await fetch('/Home/SwitchImageUrls', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ imageUrlType: urlType })
            });
            
            const result = await response.json();
            
            if (result.success) {
                console.log(`✅ Successfully switched to ${urlType} image URLs`);
                
                // Show success notification
                this.showNotification(
                    `✅ Switched to ${urlType === 'local' ? 'Local (Self-hosted)' : 'External (CDN)'} images!`, 
                    'success', 
                    4000
                );
                
                // Refresh page after a short delay to show new images
                setTimeout(() => {
                    console.log('🔄 Refreshing page to show updated images...');
                    window.location.reload();
                }, 1500);
                
            } else {
                throw new Error(result.message || 'Failed to switch image URLs');
            }
            
        } catch (error) {
            console.error('❌ Error switching image URLs:', error);
            
            this.showNotification(
                `❌ Failed to switch to ${urlType} images: ${error.message}`, 
                'error', 
                5000
            );
            
            throw error;
        }
    },

    // ✅ NEW: Enhanced Image Loading System
    setupImageLoading: function() {
        // Set up global image error handlers
        this.setupImageErrorHandlers();
        this.setupImagePreloading();
        this.setupImageFallbacks();
        
        console.log('🖼️ Enhanced image loading system initialized');
    },

    // ✅ NEW: Set up image error handlers for all images
    setupImageErrorHandlers: function() {
        // Handle images that fail to load
        document.addEventListener('error', (e) => {
            if (e.target.tagName === 'IMG') {
                this.handleImageError(e.target);
            }
        }, true);

        // Set up observers for product images
        const productImages = document.querySelectorAll('.product-image, .main-product-image');
        productImages.forEach(img => {
            this.enhanceProductImage(img);
        });
    },

    // ✅ NEW: Handle individual image errors
    handleImageError: function(img) {
        console.warn(`❌ Image failed to load: ${img.src}`);
        
        // Hide the failed image
        img.style.display = 'none';
        
        // Show the placeholder if it exists
        const placeholder = this.findImagePlaceholder(img);
        if (placeholder) {
            placeholder.style.display = 'flex';
            placeholder.classList.remove('d-none');
        }
        
        // Add error class for styling
        img.classList.add('image-error');
        
        // Try alternative image sources if available
        this.tryAlternativeImage(img);
    },

    // ✅ NEW: Find associated placeholder for an image
    findImagePlaceholder: function(img) {
        // Look for sibling placeholder
        let placeholder = img.nextElementSibling;
        if (placeholder && placeholder.classList.contains('product-image-placeholder')) {
            return placeholder;
        }
        
        // Look for placeholder in parent container
        const container = img.closest('.product-image-container');
        if (container) {
            placeholder = container.querySelector('.product-image-placeholder, .product-detail-placeholder');
            if (placeholder) return placeholder;
        }
        
        return null;
    },

    // ✅ NEW: Try alternative image sources
    tryAlternativeImage: function(img) {
        // List of fallback image services with better variety
        const fallbackServices = [
            'https://via.placeholder.com/400x300/667eea/ffffff?text=',
            'https://dummyimage.com/400x300/667eea/ffffff&text=',
            'https://fakeimg.pl/400x300/667eea/ffffff/?text=',
        ];
        
        const productName = img.alt || 'Product';
        const encodedName = encodeURIComponent(productName.replace(/[^\w\s]/gi, '').replace(/\s+/g, '+'));
        
        // Try fallback services in order
        const currentFallbackIndex = parseInt(img.getAttribute('data-fallback-index') || '0');
        
        if (currentFallbackIndex < fallbackServices.length && !img.hasAttribute('data-all-fallbacks-tried')) {
            img.setAttribute('data-fallback-index', (currentFallbackIndex + 1).toString());
            
            // Mark as tried all fallbacks if this is the last one
            if (currentFallbackIndex === fallbackServices.length - 1) {
                img.setAttribute('data-all-fallbacks-tried', 'true');
            }
            
            const newSrc = fallbackServices[currentFallbackIndex] + encodedName;
            img.src = newSrc;
            console.log(`🔄 Trying fallback ${currentFallbackIndex + 1}/${fallbackServices.length} for: ${productName}`);
            console.log(`🔗 URL: ${newSrc}`);
        } else {
            console.warn(`⚠️ All fallback services exhausted for: ${productName}`);
            // At this point, the placeholder should be shown instead
        }
    },

    // ✅ NEW: Enhance product images with loading states
    enhanceProductImage: function(img) {
        if (!img) return;
        
        // Add loading state
        img.style.opacity = '0.7';
        img.style.filter = 'blur(2px)';
        
        // Handle successful load
        img.addEventListener('load', () => {
            img.style.opacity = '1';
            img.style.filter = 'none';
            img.style.display = 'block';
            
            // Hide placeholder on successful load
            const placeholder = this.findImagePlaceholder(img);
            if (placeholder) {
                placeholder.style.display = 'none';
                placeholder.classList.add('d-none');
            }
            
            console.log(`✅ Image loaded successfully: ${img.alt || img.src}`);
        });
        
        // Handle load errors
        img.addEventListener('error', () => {
            this.handleImageError(img);
        });
    },

    // ✅ NEW: Set up image preloading for better performance
    setupImagePreloading: function() {
        // Preload images that are likely to be viewed
        const productCards = document.querySelectorAll('.product-card');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target.querySelector('.product-image, .main-product-image');
                        if (img && img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                        }
                        imageObserver.unobserve(entry.target);
                    }
                });
            }, {
                rootMargin: '50px'
            });

            productCards.forEach(card => {
                imageObserver.observe(card);
            });
        }
    },

    // ✅ NEW: Set up fallback mechanisms
    setupImageFallbacks: function() {
        // Create CSS for better image placeholders
        const imageCSS = `
            .product-image-placeholder {
                transition: all 0.3s ease;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                position: relative;
                overflow: hidden;
            }
            
            .product-image-placeholder::before {
                content: '';
                position: absolute;
                top: -50%;
                left: -50%;
                width: 200%;
                height: 200%;
                background: linear-gradient(45deg, transparent, rgba(255,255,255,0.1), transparent);
                transform: rotate(45deg);
                animation: shimmer 2s infinite;
            }
            
            @keyframes shimmer {
                0% { transform: translateX(-100%) translateY(-100%) rotate(45deg); }
                100% { transform: translateX(100%) translateY(100%) rotate(45deg); }
            }
            
            .image-error {
                display: none !important;
            }
            
            .product-image {
                transition: all 0.3s ease;
            }
            
            .product-image:hover {
                transform: scale(1.05);
            }
        `;
        
        const style = document.createElement('style');
        style.textContent = imageCSS;
        document.head.appendChild(style);
    },

    // Theme Toggle Functionality
    setupThemeToggle: function() {
        // Create theme toggle button
        const themeToggle = document.createElement('button');
        themeToggle.className = 'theme-toggle';
        themeToggle.setAttribute('aria-label', 'Toggle dark/light theme');
        themeToggle.innerHTML = '<i class="fas fa-moon"></i>';
        
        // Load saved theme or detect system preference
        const savedTheme = localStorage.getItem('gadgethub-theme');
        const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const currentTheme = savedTheme || (systemPrefersDark ? 'dark' : 'light');
        
        // Apply initial theme
        this.setTheme(currentTheme);
        this.updateThemeToggleIcon(themeToggle, currentTheme);
        
        // Add click event
        themeToggle.addEventListener('click', () => {
            const currentTheme = document.documentElement.getAttribute('data-theme') || 'light';
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
            this.setTheme(newTheme);
            this.updateThemeToggleIcon(themeToggle, newTheme);
            localStorage.setItem('gadgethub-theme', newTheme);
            
            // Add ripple effect
            this.createRippleEffect(event, themeToggle);
            
            // Show notification
            this.showNotification(
                `Switched to ${newTheme} theme`, 
                'info', 
                2000
            );
        });
        
        // Add to DOM
        document.body.appendChild(themeToggle);
        
        // Listen for system theme changes
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            if (!localStorage.getItem('gadgethub-theme')) {
                const newTheme = e.matches ? 'dark' : 'light';
                this.setTheme(newTheme);
                this.updateThemeToggleIcon(themeToggle, newTheme);
            }
        });
    },

    setTheme: function(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        
        // Update meta theme-color for mobile browsers
        let metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (!metaThemeColor) {
            metaThemeColor = document.createElement('meta');
            metaThemeColor.name = 'theme-color';
            document.head.appendChild(metaThemeColor);
        }
        
        metaThemeColor.content = theme === 'dark' ? '#0f172a' : '#10b981';
        
        // Add smooth transition class
        document.body.classList.add('theme-transitioning');
        setTimeout(() => {
            document.body.classList.remove('theme-transitioning');
        }, 300);
    },

    updateThemeToggleIcon: function(toggle, theme) {
        const icon = toggle.querySelector('i');
        if (theme === 'dark') {
            icon.className = 'fas fa-sun';
            toggle.setAttribute('aria-label', 'Switch to light theme');
        } else {
            icon.className = 'fas fa-moon';
            toggle.setAttribute('aria-label', 'Switch to dark theme');
        }
    },

    // Enhanced Product Detail Page functionality
    setupProductDetailEnhancements: function() {
        if (document.querySelector('.product-detail-page')) {
            this.setupImageGallery();
            this.setupProductInteractions();
            this.setupQuantityControls();
        }
    },

    // Enhanced Image Gallery
    setupImageGallery: function() {
        const mainImage = document.querySelector('.main-product-image');
        const thumbnails = document.querySelectorAll('.thumbnail');
        
        if (mainImage && thumbnails.length > 0) {
            // Add zoom functionality
            this.addImageZoom(mainImage);
            
            // Enhanced thumbnail interactions
            thumbnails.forEach((thumbnail, index) => {
                thumbnail.addEventListener('click', (e) => {
                    this.switchMainImage(e.target.src, thumbnails);
                    this.createRippleEffect(e, thumbnail);
                });
                
                // Add loading animation
                thumbnail.addEventListener('load', () => {
                    thumbnail.style.opacity = '1';
                });
            });
        }
    },

    // Add zoom functionality to main image
    addImageZoom: function(image) {
        let isZoomed = false;
        
        image.addEventListener('click', () => {
            if (!isZoomed) {
                this.showImageModal(image.src, image.alt);
            }
        });
        
        // Add zoom cursor
        image.style.cursor = 'zoom-in';
    },

    // Show image in modal
    showImageModal: function(src, alt) {
        const modal = document.createElement('div');
        modal.className = 'image-modal';
        modal.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.9);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 2000;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        
        const img = document.createElement('img');
        img.src = src;
        img.alt = alt;
        img.style.cssText = `
            max-width: 90%;
            max-height: 90%;
            object-fit: contain;
            border-radius: 12px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.5);
            transform: scale(0.8);
            transition: transform 0.3s ease;
        `;
        
        const closeBtn = document.createElement('button');
        closeBtn.innerHTML = '×';
        closeBtn.style.cssText = `
            position: absolute;
            top: 20px;
            right: 30px;
            background: none;
            border: none;
            color: white;
            font-size: 3rem;
            cursor: pointer;
            z-index: 2001;
            transition: transform 0.2s ease;
        `;
        
        closeBtn.addEventListener('click', () => this.closeImageModal(modal));
        closeBtn.addEventListener('mouseenter', () => closeBtn.style.transform = 'scale(1.1)');
        closeBtn.addEventListener('mouseleave', () => closeBtn.style.transform = 'scale(1)');
        
        modal.appendChild(img);
        modal.appendChild(closeBtn);
        document.body.appendChild(modal);
        
        // Animate in
        setTimeout(() => {
            modal.style.opacity = '1';
            img.style.transform = 'scale(1)';
        }, 10);
        
        // Close on background click
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                this.closeImageModal(modal);
            }
        });
        
        // Close on escape key
        const escapeHandler = (e) => {
            if (e.key === 'Escape') {
                this.closeImageModal(modal);
                document.removeEventListener('keydown', escapeHandler);
            }
        };
        document.addEventListener('keydown', escapeHandler);
    },

    // Close image modal
    closeImageModal: function(modal) {
        modal.style.opacity = '0';
        modal.querySelector('img').style.transform = 'scale(0.8)';
        setTimeout(() => {
            if (modal.parentElement) {
                modal.remove();
            }
        }, 300);
    },

    // Switch main image with animation
    switchMainImage: function(newSrc, thumbnails) {
        const mainImage = document.querySelector('.main-product-image');
        const placeholder = document.querySelector('.product-detail-placeholder');
        
        if (mainImage && newSrc) {
            // Add loading state
            mainImage.style.opacity = '0.5';
            
            // Create new image to preload
            const newImage = new Image();
            newImage.onload = () => {
                mainImage.src = newSrc;
                mainImage.style.opacity = '1';
                mainImage.style.display = 'block';
                if (placeholder) placeholder.style.display = 'none';
            };
            newImage.onerror = () => {
                mainImage.style.opacity = '1';
                mainImage.style.display = 'none';
                if (placeholder) placeholder.style.display = 'flex';
            };
            newImage.src = newSrc;
            
            // Update active thumbnail
            thumbnails.forEach(thumb => {
                thumb.classList.remove('active');
                thumb.style.borderColor = '#e2e8f0';
                thumb.style.transform = 'scale(1)';
            });
            
            // Find and activate current thumbnail
            const activeThumbnail = Array.from(thumbnails).find(thumb => thumb.src === newSrc);
            if (activeThumbnail) {
                activeThumbnail.classList.add('active');
                activeThumbnail.style.borderColor = '#10b981';
                activeThumbnail.style.transform = 'scale(1.05)';
            }
        }
    },

    // Enhanced Product Interactions
    setupProductInteractions: function() {
        // Enhanced Add to Cart
        const addToCartBtns = document.querySelectorAll('[onclick*="addToCart"]');
        addToCartBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.createRippleEffect(e, btn);
            });
        });
        
        // Enhanced Wishlist
        const wishlistBtns = document.querySelectorAll('.btn-outline-warning');
        wishlistBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.toggleWishlist(e, btn);
            });
        });
    },

    // Enhanced Quantity Controls
    setupQuantityControls: function() {
        const quantityInput = document.getElementById('quantity');
        const minusBtn = document.querySelector('[onclick*="changeQuantity(-1)"]');
        const plusBtn = document.querySelector('[onclick*="changeQuantity(1)"]');
        
        if (quantityInput && minusBtn && plusBtn) {
            // Enhanced change quantity function
            window.changeQuantity = (change) => {
                const currentValue = parseInt(quantityInput.value);
                const newValue = currentValue + change;
                
                if (newValue >= 1 && newValue <= 100) {
                    // Animate the change
                    quantityInput.style.transform = 'scale(1.1)';
                    quantityInput.style.color = change > 0 ? '#10b981' : '#ef4444';
                    
                    setTimeout(() => {
                        quantityInput.value = newValue;
                        quantityInput.style.transform = 'scale(1)';
                        quantityInput.style.color = '';
                    }, 150);
                    
                    // Update price if available
                    this.updateTotalPrice(newValue);
                }
                
                // Button feedback
                const btnToAnimate = change > 0 ? plusBtn : minusBtn;
                btnToAnimate.style.transform = 'scale(0.95)';
                setTimeout(() => btnToAnimate.style.transform = 'scale(1)', 100);
            };
            
            // Keyboard support
            quantityInput.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    changeQuantity(1);
                } else if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    changeQuantity(-1);
                }
            });
        }
    },

    // Update total price (if applicable)
    updateTotalPrice: function(quantity) {
        const priceElement = document.querySelector('.current-price');
        if (priceElement) {
            // This would calculate total based on unit price
            // For now, just add visual feedback
            priceElement.style.transform = 'scale(1.05)';
            priceElement.style.color = '#10b981';
            setTimeout(() => {
                priceElement.style.transform = 'scale(1)';
                priceElement.style.color = '';
            }, 300);
        }
    },

    // Toggle Wishlist with animation
    toggleWishlist: function(event, btn) {
        event.preventDefault();
        
        const icon = btn.querySelector('i');
        const isActive = btn.classList.contains('active');
        
        if (!isActive) {
            // Add to wishlist animation
            icon.className = 'fas fa-heart me-2';
            btn.classList.add('active');
            btn.classList.remove('btn-outline-warning');
            btn.classList.add('btn-warning');
            
            // Heart animation
            icon.style.transform = 'scale(1.3)';
            icon.style.color = '#dc2626';
            setTimeout(() => {
                icon.style.transform = 'scale(1)';
                icon.style.color = '';
            }, 300);
            
            this.showNotification('Added to wishlist!', 'success');
        } else {
            // Remove from wishlist
            icon.className = 'far fa-heart me-2';
            btn.classList.remove('active');
            btn.classList.add('btn-outline-warning');
            btn.classList.remove('btn-warning');
            
            this.showNotification('Removed from wishlist', 'info');
        }
        
        this.createRippleEffect(event, btn);
    },

    // Enhanced Search functionality
    setupSearch: function () {
        const searchInput = document.querySelector('input[name="SearchQuery"], input[placeholder*="Search"]');

        if (searchInput) {
            let searchTimeout;

            searchInput.addEventListener('input', function () {
                const query = this.value.trim();

                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    if (query.length >= 2) {
                        GadgetHub.showSearchSuggestions(searchInput, query);
                    } else {
                        GadgetHub.hideSearchSuggestions();
                    }
                }, 300);
            });

            // Hide suggestions when clicking outside
            document.addEventListener('click', function (e) {
                if (!e.target.closest('.search-container, .search-box')) {
                    GadgetHub.hideSearchSuggestions();
                }
            });

            // Enhanced keyboard navigation
            searchInput.addEventListener('keydown', function(e) {
                const suggestions = document.querySelectorAll('.search-suggestion-item');
                const activeSuggestion = document.querySelector('.search-suggestion-item.active');
                
                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    if (activeSuggestion) {
                        activeSuggestion.classList.remove('active');
                        const next = activeSuggestion.nextElementSibling;
                        if (next) next.classList.add('active');
                        else suggestions[0]?.classList.add('active');
                    } else {
                        suggestions[0]?.classList.add('active');
                    }
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    if (activeSuggestion) {
                        activeSuggestion.classList.remove('active');
                        const prev = activeSuggestion.previousElementSibling;
                        if (prev) prev.classList.add('active');
                        else suggestions[suggestions.length - 1]?.classList.add('active');
                    } else {
                        suggestions[suggestions.length - 1]?.classList.add('active');
                    }
                } else if (e.key === 'Enter' && activeSuggestion) {
                    e.preventDefault();
                    activeSuggestion.click();
                }
            });
        }
    },

    showSearchSuggestions: function (input, query) {
        // Enhanced search suggestions with product data
        const suggestions = [
            { name: 'iPhone 15 Pro Max', category: 'Smartphones', image: '📱' },
            { name: 'Samsung Galaxy S24 Ultra', category: 'Smartphones', image: '📱' },
            { name: 'MacBook Pro M3 Max', category: 'Laptops', image: '💻' },
            { name: 'iPad Pro M4', category: 'Tablets', image: '📱' },
            { name: 'AirPods Pro', category: 'Accessories', image: '🎧' },
            { name: 'Apple Watch Ultra 2', category: 'Smart Watches', image: '⌚' }
        ].filter(item => item.name.toLowerCase().includes(query.toLowerCase()));

        if (suggestions.length > 0) {
            this.renderSearchSuggestions(input, suggestions);
        } else {
            this.hideSearchSuggestions();
        }
    },

    renderSearchSuggestions: function (input, suggestions) {
        this.hideSearchSuggestions();

        const container = document.createElement('div');
        container.className = 'search-suggestions';
        container.style.cssText = `
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: var(--card-bg);
            border: 2px solid var(--border-primary);
            border-radius: 12px;
            box-shadow: var(--shadow-lg);
            z-index: 1000;
            max-height: 300px;
            overflow-y: auto;
            border-top: none;
            margin-top: 2px;
        `;

        suggestions.forEach((suggestion, index) => {
            const item = document.createElement('div');
            item.className = 'search-suggestion-item';
            item.style.cssText = `
                padding: 1rem 1.25rem;
                cursor: pointer;
                border-bottom: 1px solid var(--border-primary);
                transition: all 0.2s ease;
                display: flex;
                align-items: center;
                gap: 0.75rem;
                color: var(--text-primary);
            `;
            
            item.innerHTML = `
                <span style="font-size: 1.5rem;">${suggestion.image}</span>
                <div>
                    <div style="font-weight: 600; color: var(--text-primary);">${suggestion.name}</div>
                    <div style="font-size: 0.875rem; color: var(--text-secondary);">${suggestion.category}</div>
                </div>
            `;

            item.addEventListener('mouseenter', function () {
                document.querySelectorAll('.search-suggestion-item').forEach(i => i.classList.remove('active'));
                this.classList.add('active');
                this.style.backgroundColor = 'var(--bg-secondary)';
                this.style.borderColor = 'var(--primary-color)';
            });

            item.addEventListener('mouseleave', function () {
                this.style.backgroundColor = '';
                this.style.borderColor = '';
            });

            item.addEventListener('click', function () {
                input.value = suggestion.name;
                input.form?.submit();
            });

            container.appendChild(item);
        });

        input.parentElement.style.position = 'relative';
        input.parentElement.appendChild(container);

        // Animate in
        setTimeout(() => {
            container.style.transform = 'translateY(0)';
            container.style.opacity = '1';
        }, 10);
    },

    hideSearchSuggestions: function () {
        const suggestions = document.querySelector('.search-suggestions');
        if (suggestions) {
            suggestions.style.opacity = '0';
            suggestions.style.transform = 'translateY(-10px)';
            setTimeout(() => suggestions.remove(), 200);
        }
    },

    // Enhanced Cart functionality
    setupCart: function () {
        this.updateCartCount();
        this.setupCartNotifications();
        this.setupCartAnimations();
    },

    setupCartAnimations: function() {
        // Animate cart icon when items are added
        const cartIcons = document.querySelectorAll('.fa-shopping-cart');
        cartIcons.forEach(icon => {
            icon.addEventListener('animationend', () => {
                icon.classList.remove('cart-bounce');
            });
        });
    },

    updateCartCount: function () {
        const cartCount = this.getCartCount();
        const cartBadges = document.querySelectorAll('.cart-count');

        cartBadges.forEach(badge => {
            badge.textContent = cartCount;
            badge.style.display = cartCount > 0 ? 'inline-block' : 'none';
            
            if (cartCount > 0) {
                badge.classList.add('cart-bounce');
            }
        });
    },

    getCartCount: function () {
        const sessionCart = sessionStorage.getItem('gadgethub_cart_count');
        return sessionCart ? parseInt(sessionCart) : 0;
    },

    addToCart: function(productId, quantity = 1) {
        const currentCount = this.getCartCount();
        const newCount = currentCount + quantity;
        
        sessionStorage.setItem('gadgethub_cart_count', newCount.toString());
        this.updateCartCount();
        
        // Animate cart icon
        const cartIcons = document.querySelectorAll('.fa-shopping-cart');
        cartIcons.forEach(icon => {
            icon.classList.add('cart-bounce');
        });
        
        this.showNotification(`Added ${quantity} item(s) to cart!`, 'success');
    },

    setupCartNotifications: function () {
        // Listen for cart update events
        document.addEventListener('cartUpdated', (e) => {
            this.updateCartCount();
            this.showNotification(e.detail.message, e.detail.type || 'success');
        });
    },

    // Enhanced Navigation
    setupNavigation: function () {
        this.setupMobileNavigation();
        this.setupScrollEffects();
        this.setupActiveNavHighlight();
    },

    setupMobileNavigation: function () {
        const navbarToggler = document.querySelector('.navbar-toggler');
        const navbarCollapse = document.querySelector('.navbar-collapse');

        if (navbarToggler && navbarCollapse) {
            navbarToggler.addEventListener('click', function () {
                navbarCollapse.classList.toggle('show');
            });
        }
    },

    setupScrollEffects: function () {
        let lastScroll = 0;
        const navbar = document.querySelector('.navbar');

        if (navbar) {
            window.addEventListener('scroll', () => {
                const currentScroll = window.pageYOffset;

                if (currentScroll > lastScroll && currentScroll > 100) {
                    navbar.style.transform = 'translateY(-100%)';
                } else {
                    navbar.style.transform = 'translateY(0)';
                }

                lastScroll = currentScroll;
            });
        }
    },

    setupActiveNavHighlight: function () {
        const navLinks = document.querySelectorAll('.nav-link');
        const currentPath = window.location.pathname;

        navLinks.forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });
    },

    // Enhanced Notifications
    setupNotifications: function () {
        this.createNotificationContainer();
    },

    createNotificationContainer: function () {
        if (!document.getElementById('notification-container')) {
            const container = document.createElement('div');
            container.id = 'notification-container';
            container.style.cssText = `
                position: fixed;
                top: 100px;
                right: 20px;
                z-index: 1060;
                max-width: 400px;
            `;
            document.body.appendChild(container);
        }
    },

    showNotification: function (message, type = 'info', duration = 5000) {
        const container = document.getElementById('notification-container');
        if (!container) return;

        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show mb-2`;
        notification.style.cssText = `
            box-shadow: var(--shadow-lg);
            border: none;
            border-radius: 12px;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            background-color: var(--card-bg);
            color: var(--text-primary);
            border: 1px solid var(--border-primary);
        `;

        const iconMap = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-triangle',
            warning: 'fa-exclamation-triangle',
            info: 'fa-info-circle'
        };

        notification.innerHTML = `
            <i class="fas ${iconMap[type]} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        container.appendChild(notification);

        // Animate in
        setTimeout(() => notification.style.transform = 'translateX(0)', 100);

        // Auto remove
        setTimeout(() => {
            if (notification.parentElement) {
                notification.style.transform = 'translateX(100%)';
                setTimeout(() => notification.remove(), 300);
            }
        }, duration);

        // Manual close
        const closeBtn = notification.querySelector('.btn-close');
        closeBtn.addEventListener('click', () => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        });
    },

    // Enhanced Lazy Loading
    setupLazyLoading: function () {
        const images = document.querySelectorAll('img[data-src]');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.remove('lazy');
                        observer.unobserve(img);
                    }
                });
            });

            images.forEach(img => imageObserver.observe(img));
        } else {
            // Fallback for older browsers
            images.forEach(img => {
                img.src = img.dataset.src;
            });
        }
    },

    // Utility Functions
    createRippleEffect: function (event, element) {
        const ripple = document.createElement('span');
        const rect = element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        ripple.style.cssText = `
            position: absolute;
            width: ${size}px;
            height: ${size}px;
            left: ${x}px;
            top: ${y}px;
            background: rgba(255, 255, 255, 0.5);
            border-radius: 50%;
            transform: scale(0);
            animation: ripple 0.6s linear;
            pointer-events: none;
            z-index: 1;
        `;

        element.style.position = element.style.position || 'relative';
        element.style.overflow = 'hidden';
        element.appendChild(ripple);

        setTimeout(() => ripple.remove(), 600);
    },

    // Format currency
    formatCurrency: function (amount, currency = 'USD') {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency
        }).format(amount);
    },

    // Debounce utility
    debounce: function (func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};

// Add ripple animation CSS and theme transition styles
const rippleCSS = `
    @keyframes ripple {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    @keyframes cart-bounce {
        0%, 20%, 60%, 100% {
            transform: translateY(0);
        }
        40% {
            transform: translateY(-10px);
        }
        80% {
            transform: translateY(-5px);
        }
    }
    
    .cart-bounce {
        animation: cart-bounce 0.6s ease;
    }
    
    .search-suggestion-item.active {
        background-color: var(--bg-secondary) !important;
        border-left: 4px solid var(--primary-color) !important;
    }
    
    .theme-transitioning * {
        transition: background-color 0.3s ease, color 0.3s ease, border-color 0.3s ease !important;
    }
`;

const style = document.createElement('style');
style.textContent = rippleCSS;
document.head.appendChild(style);

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    GadgetHub.init();
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GadgetHub;
}