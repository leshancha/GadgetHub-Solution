/**
 * Gadget Hub Dashboard JavaScript
 * Built by leshancha at 2025-07-31 10:46:30 UTC
 */

class GadgetHubDashboard {
    constructor() {
        this.init();
        this.bindEvents();
        this.startRealtimeUpdates();
    }

    init() {
        console.log('🚀 Gadget Hub Dashboard initialized by leshancha at 2025-07-31 10:46:30 UTC');
        this.setupTooltips();
        this.setupAnimations();
        this.setupCharts();
        this.updateTimestamps();
    }

    bindEvents() {
        // Mobile sidebar toggle
        this.setupMobileSidebar();

        // Quick action handlers
        this.setupQuickActions();

        // Stats card interactions
        this.setupStatsCards();

        // Notification handlers
        this.setupNotifications();

        // Search and filters
        this.setupSearch();

        // Auto-refresh controls
        this.setupAutoRefresh();
    }

    setupTooltips() {
        // Initialize Bootstrap tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl, {
                delay: { show: 500, hide: 100 }
            });
        });
    }

    setupAnimations() {
        // Animate dashboard cards on load
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '0';
                    entry.target.style.transform = 'translateY(20px)';
                    entry.target.style.transition = 'all 0.6s ease';

                    setTimeout(() => {
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                    }, 100);
                }
            });
        });

        document.querySelectorAll('.stats-card, .quick-action-card, .management-card').forEach(card => {
            observer.observe(card);
        });
    }

    setupCharts() {
        // Revenue Chart (if exists)
        const revenueChart = document.getElementById('revenueChart');
        if (revenueChart) {
            this.initRevenueChart(revenueChart);
        }

        // Orders Chart (if exists)
        const ordersChart = document.getElementById('ordersChart');
        if (ordersChart) {
            this.initOrdersChart(ordersChart);
        }

        // Customer Analytics Chart (if exists)
        const analyticsChart = document.getElementById('analyticsChart');
        if (analyticsChart) {
            this.initAnalyticsChart(analyticsChart);
        }
    }

    initRevenueChart(canvas) {
        const ctx = canvas.getContext('2d');

        // Generate dynamic data based on current date
        const currentMonth = new Date().getMonth();
        const monthlyData = this.generateMonthlyData(currentMonth);

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                datasets: [{
                    label: 'Revenue ($)',
                    data: monthlyData.revenue,
                    borderColor: '#10b981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#10b981',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 6,
                    pointHoverRadius: 8
                }, {
                    label: 'Orders',
                    data: monthlyData.orders,
                    borderColor: '#22c55e',
                    backgroundColor: 'rgba(34, 197, 94, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    yAxisID: 'y1',
                    pointBackgroundColor: '#22c55e',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 6,
                    pointHoverRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            usePointStyle: true,
                            padding: 20,
                            font: {
                                family: 'Inter',
                                size: 12,
                                weight: '500'
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: 'Revenue & Orders Performance - 2025',
                        font: {
                            family: 'Inter',
                            size: 16,
                            weight: '600'
                        },
                        padding: 20
                    }
                },
                scales: {
                    y: {
                        type: 'linear',
                        display: true,
                        position: 'left',
                        title: {
                            display: true,
                            text: 'Revenue ($)',
                            font: {
                                family: 'Inter',
                                size: 12,
                                weight: '500'
                            }
                        },
                        grid: {
                            color: 'rgba(0, 0, 0, 0.1)'
                        }
                    },
                    y1: {
                        type: 'linear',
                        display: true,
                        position: 'right',
                        title: {
                            display: true,
                            text: 'Orders',
                            font: {
                                family: 'Inter',
                                size: 12,
                                weight: '500'
                            }
                        },
                        grid: {
                            drawOnChartArea: false,
                        },
                    },
                    x: {
                        grid: {
                            color: 'rgba(0, 0, 0, 0.1)'
                        }
                    }
                },
                animation: {
                    duration: 2000,
                    easing: 'easeInOutQuart'
                }
            }
        });
    }

    generateMonthlyData(currentMonth) {
        const revenue = [];
        const orders = [];

        for (let i = 0; i < 12; i++) {
            // Generate realistic revenue data (10k - 50k range)
            const baseRevenue = 15000 + (Math.sin(i * 0.5) * 10000) + (Math.random() * 15000);
            revenue.push(Math.round(baseRevenue));

            // Generate orders data (50 - 200 range)
            const baseOrders = 80 + (Math.sin(i * 0.7) * 40) + (Math.random() * 60);
            orders.push(Math.round(baseOrders));
        }

        return { revenue, orders };
    }

    setupMobileSidebar() {
        // Create mobile toggle button
        const navbar = document.querySelector('.navbar');
        if (navbar && window.innerWidth <= 992) {
            const toggleBtn = document.createElement('button');
            toggleBtn.className = 'btn btn-outline-light d-lg-none';
            toggleBtn.innerHTML = '<i class="fas fa-bars"></i>';
            toggleBtn.onclick = () => this.toggleSidebar();

            const navbarBrand = navbar.querySelector('.navbar-brand');
            navbarBrand.parentNode.insertBefore(toggleBtn, navbarBrand.nextSibling);
        }

        // Create floating action button for mobile
        this.createFloatingActionButton();
    }

    createFloatingActionButton() {
        if (window.innerWidth <= 992) {
            const fab = document.createElement('button');
            fab.className = 'dashboard-fab';
            fab.innerHTML = '<i class="fas fa-bars"></i>';
            fab.onclick = () => this.toggleSidebar();
            document.body.appendChild(fab);
        }
    }

    toggleSidebar() {
        const sidebar = document.querySelector('.sidebar');
        if (sidebar) {
            sidebar.classList.toggle('show');

            // Add backdrop for mobile
            if (sidebar.classList.contains('show')) {
                this.createBackdrop();
            } else {
                this.removeBackdrop();
            }
        }
    }

    createBackdrop() {
        const backdrop = document.createElement('div');
        backdrop.className = 'sidebar-backdrop';
        backdrop.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            z-index: 999;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        backdrop.onclick = () => this.toggleSidebar();
        document.body.appendChild(backdrop);

        setTimeout(() => backdrop.style.opacity = '1', 10);
    }

    removeBackdrop() {
        const backdrop = document.querySelector('.sidebar-backdrop');
        if (backdrop) {
            backdrop.style.opacity = '0';
            setTimeout(() => backdrop.remove(), 300);
        }
    }

    setupQuickActions() {
        // Add click animations to quick action cards
        document.querySelectorAll('.quick-action-card, .management-card, .gaming-action-card').forEach(card => {
            card.addEventListener('click', (e) => {
                // Create ripple effect
                this.createRippleEffect(e, card);

                // Add success animation if it's a form submission
                if (card.querySelector('form')) {
                    e.preventDefault();
                    this.handleFormSubmission(card);
                }
            });
        });
    }

    createRippleEffect(event, element) {
        const ripple = document.createElement('span');
        const rect = element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        // Adjust ripple color based on element type
        let rippleColor = 'rgba(16, 185, 129, 0.3)';
        if (element.classList.contains('gaming-action-card')) {
            rippleColor = 'rgba(255, 0, 128, 0.4)';
        } else if (element.classList.contains('gaming-stats-card')) {
            rippleColor = 'rgba(0, 255, 136, 0.4)';
        }

        ripple.style.cssText = `
            position: absolute;
            width: ${size}px;
            height: ${size}px;
            left: ${x}px;
            top: ${y}px;
            background: ${rippleColor};
            border-radius: 50%;
            transform: scale(0);
            animation: ripple 0.6s linear;
            pointer-events: none;
        `;

        element.style.position = 'relative';
        element.style.overflow = 'hidden';
        element.appendChild(ripple);

        setTimeout(() => ripple.remove(), 600);
    }

    // Handle form submissions within action cards
    handleFormSubmission(card) {
        const form = card.querySelector('form');
        if (!form) return;

        // Show loading state
        const originalContent = card.innerHTML;
        card.classList.add('submitting');
        
        // Create loading overlay
        const loadingOverlay = document.createElement('div');
        loadingOverlay.className = 'card-loading-overlay';
        loadingOverlay.style.cssText = `
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.7);
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-weight: bold;
            z-index: 10;
            border-radius: inherit;
        `;
        loadingOverlay.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
        card.appendChild(loadingOverlay);

        // Simulate form submission
        setTimeout(() => {
            // Remove loading overlay
            loadingOverlay.remove();
            card.classList.remove('submitting');
            
            // Show success state
            this.showCardSuccess(card);
            
            // Reset after delay
            setTimeout(() => {
                card.innerHTML = originalContent;
                // Re-bind events for the restored content
                this.setupQuickActions();
            }, 2000);
        }, 1500);
    }

    // Show success state for action cards
    showCardSuccess(card) {
        card.classList.add('success-state');
        const successOverlay = document.createElement('div');
        successOverlay.className = 'card-success-overlay';
        successOverlay.style.cssText = `
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: linear-gradient(135deg, rgba(16, 185, 129, 0.9) 0%, rgba(34, 197, 94, 0.9) 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-weight: bold;
            z-index: 10;
            border-radius: inherit;
            animation: successPulse 0.5s ease-out;
        `;
        successOverlay.innerHTML = '<i class="fas fa-check-circle fa-2x me-2"></i>Success!';
        card.appendChild(successOverlay);
    }

    setupStatsCards() {
        // Add hover effects and click handlers for stats cards
        document.querySelectorAll('.stats-card, .gaming-stats-card').forEach(card => {
            card.addEventListener('mouseenter', () => {
                this.animateStatsNumber(card);
            });

            // Make stats cards clickable
            const link = card.querySelector('.stats-footer a, a');
            if (link) {
                card.style.cursor = 'pointer';
                card.addEventListener('click', (e) => {
                    if (e.target !== link && !e.target.closest('a')) {
                        this.createRippleEffect(e, card);
                        // Delay click to show ripple effect
                        setTimeout(() => {
                            link.click();
                        }, 150);
                    }
                });
            }

            // Add gaming-specific interactions
            if (card.classList.contains('gaming-stats-card')) {
                this.setupGamingCardInteractions(card);
            }
        });
    }

    // Setup special gaming card interactions
    setupGamingCardInteractions(card) {
        const number = card.querySelector('.gaming-number');
        const icon = card.querySelector('.gaming-icon');
        
        // Particle effect on hover
        card.addEventListener('mouseenter', () => {
            this.createGamingParticles(card);
        });

        // Number pulsing effect
        if (number) {
            setInterval(() => {
                if (Math.random() > 0.7) { // 30% chance every interval
                    number.style.transform = 'scale(1.05)';
                    setTimeout(() => {
                        number.style.transform = 'scale(1)';
                    }, 200);
                }
            }, 3000);
        }

        // Icon glow cycling
        if (icon) {
            let glowIntensity = 0.5;
            let glowDirection = 1;
            
            setInterval(() => {
                glowIntensity += 0.1 * glowDirection;
                if (glowIntensity >= 1) glowDirection = -1;
                if (glowIntensity <= 0.5) glowDirection = 1;
                
                icon.style.boxShadow = `0 0 ${20 + (glowIntensity * 10)}px rgba(0, 255, 255, ${glowIntensity})`;
            }, 200);
        }
    }

    // Create gaming particle effects
    createGamingParticles(container) {
        const particleCount = 8;
        
        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'gaming-particle';
            
            const size = Math.random() * 6 + 2;
            const x = Math.random() * container.offsetWidth;
            const y = Math.random() * container.offsetHeight;
            const color = ['#00ff88', '#00ffff', '#ff0080'][Math.floor(Math.random() * 3)];
            
            particle.style.cssText = `
                position: absolute;
                width: ${size}px;
                height: ${size}px;
                background: ${color};
                border-radius: 50%;
                left: ${x}px;
                top: ${y}px;
                pointer-events: none;
                animation: particleFloat ${1 + Math.random() * 2}s ease-out forwards;
                z-index: 1;
            `;
            
            container.appendChild(particle);
            
            setTimeout(() => {
                if (particle.parentNode) {
                    particle.remove();
                }
            }, 3000);
        }
    }

    checkNotifications() {
        // This would typically make an API call
        console.log('🔔 Checking for new notifications...');

        // Simulate new notifications
        if (Math.random() > 0.8) {
            this.showNotificationBadge();
        }
    }

    showNotificationBadge() {
        const bellIcon = document.querySelector('.fa-bell');
        if (bellIcon && !bellIcon.parentElement.querySelector('.notification-badge')) {
            const badge = document.createElement('span');
            badge.className = 'notification-badge';
            badge.textContent = '1';
            bellIcon.parentElement.style.position = 'relative';
            bellIcon.parentElement.appendChild(badge);
        }
    }

    handleNotificationClick(notification) {
        // Mark as read and remove badge
        notification.style.opacity = '0.7';
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            badge.remove();
        }
    }

    setupSearch() {
        // Global dashboard search
        const searchInput = document.querySelector('#dashboardSearch');
        if (searchInput) {
            let searchTimeout;

            searchInput.addEventListener('input', (e) => {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    this.performSearch(e.target.value);
                }, 300);
            });
        }
    }

    performSearch(query) {
        if (!query) {
            this.clearSearchResults();
            return;
        }

        console.log(`🔍 Searching for: ${query}`);

        // This would typically call your API
        // For now, just highlight matching elements
        this.highlightSearchResults(query);
    }

    highlightSearchResults(query) {
        const elements = document.querySelectorAll('.stats-card, .quick-action-card, .management-card');
        elements.forEach(element => {
            const text = element.textContent.toLowerCase();
            if (text.includes(query.toLowerCase())) {
                element.style.border = '2px solid #10b981';
                element.style.boxShadow = '0 0 20px rgba(16, 185, 129, 0.3)';
            } else {
                element.style.opacity = '0.5';
            }
        });
    }

    clearSearchResults() {
        const elements = document.querySelectorAll('.stats-card, .quick-action-card, .management-card');
        elements.forEach(element => {
            element.style.border = '';
            element.style.boxShadow = '';
            element.style.opacity = '';
        });
    }

    setupAutoRefresh() {
        // Auto-refresh dashboard data
        let refreshInterval = 300000; // 5 minutes default

        const refreshBtn = document.querySelector('#refreshDashboard');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                this.refreshDashboardData();
            });
        }

        // Start auto-refresh
        setInterval(() => {
            this.refreshDashboardData(true);
        }, refreshInterval);
    }

    refreshDashboardData(silent = false) {
        if (!silent) {
            this.showRefreshIndicator();
        }

        console.log('🔄 Refreshing dashboard data...');

        // Simulate API call
        setTimeout(() => {
            this.updateTimestamps();
            this.updateStatsCards();

            if (!silent) {
                this.hideRefreshIndicator();
                this.showSuccessMessage('Dashboard updated successfully!');
            }
        }, 1000);
    }

    showRefreshIndicator() {
        const indicator = document.createElement('div');
        indicator.id = 'refreshIndicator';
        indicator.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: #10b981;
            color: white;
            padding: 0.5rem 1rem;
            border-radius: 8px;
            z-index: 1060;
            display: flex;
            align-items: center;
            gap: 0.5rem;
            box-shadow: 0 4px 20px rgba(16, 185, 129, 0.3);
        `;
        indicator.innerHTML = '<i class="fas fa-sync fa-spin"></i> Updating...';
        document.body.appendChild(indicator);
    }

    hideRefreshIndicator() {
        const indicator = document.getElementById('refreshIndicator');
        if (indicator) {
            indicator.remove();
        }
    }

    showSuccessMessage(message) {
        const toast = document.createElement('div');
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: #10b981;
            color: white;
            padding: 0.75rem 1rem;
            border-radius: 8px;
            z-index: 1060;
            display: flex;
            align-items: center;
            gap: 0.5rem;
            box-shadow: 0 4px 20px rgba(16, 185, 129, 0.3);
            transform: translateX(100%);
            transition: transform 0.3s ease;
        `;
        toast.innerHTML = `<i class="fas fa-check"></i> ${message}`;
        document.body.appendChild(toast);

        setTimeout(() => toast.style.transform = 'translateX(0)', 100);
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    updateTimestamps() {
        const timestampElements = document.querySelectorAll('[data-timestamp]');
        timestampElements.forEach(element => {
            const timestamp = new Date().toLocaleString();
            element.textContent = timestamp;
        });

        // Update "Last updated" times
        const lastUpdatedElements = document.querySelectorAll('.last-updated');
        lastUpdatedElements.forEach(element => {
            element.textContent = `Last updated: ${new Date().toLocaleTimeString()} UTC`;
        });
    }

    updateStatsCards() {
        // Simulate stats updates with small random changes
        const statsNumbers = document.querySelectorAll('.stats-number');
        statsNumbers.forEach(numberEl => {
            const currentValue = parseInt(numberEl.textContent.replace(/[^0-9]/g, '')) || 0;
            const change = Math.floor(Math.random() * 5) - 2; // -2 to +2
            const newValue = Math.max(0, currentValue + change);

            if (newValue !== currentValue) {
                numberEl.classList.add('success-animation');
                this.countUpAnimation(numberEl, currentValue, newValue, 500);

                setTimeout(() => {
                    numberEl.classList.remove('success-animation');
                }, 600);
            }
        });
    }

    // Add missing count up animation method
    countUpAnimation(element, start, end, duration) {
        const startTime = performance.now();
        const startValue = start;
        const endValue = end;
        
        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            
            const currentValue = Math.floor(startValue + (endValue - startValue) * progress);
            
            // Format number with commas if needed
            if (element.textContent.includes('$')) {
                element.textContent = '$' + currentValue.toLocaleString();
            } else {
                element.textContent = currentValue.toLocaleString();
            }
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };
        
        requestAnimationFrame(animate);
    }

    // Add missing animation for stats numbers
    animateStatsNumber(card) {
        const numberEl = card.querySelector('.stats-number');
        if (numberEl) {
            numberEl.style.transform = 'scale(1.1)';
            numberEl.style.transition = 'transform 0.3s ease';
            
            setTimeout(() => {
                numberEl.style.transform = 'scale(1)';
            }, 300);
        }
    }

    // Enhanced notification setup
    setupNotifications() {
        // Check for notifications every 30 seconds
        setInterval(() => {
            this.checkNotifications();
        }, 30000);

        // Handle notification clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('.notification-item')) {
                this.handleNotificationClick(e.target.closest('.notification-item'));
            }
        });
    }
}

/* Gaming particle effects */
@keyframes particleFloat {
    0% {
        opacity: 0;
        transform: translateY(0) scale(0);
    }
    20% {
        opacity: 1;
        transform: translateY(-10px) scale(1);
    }
    80% {
        opacity: 1;
        transform: translateY(-40px) scale(1);
    }
    100% {
        opacity: 0;
        transform: translateY(-60px) scale(0);
    }
}

/* Enhanced gaming number animation */
.gaming-number {
    transition: transform 0.2s ease, text-shadow 0.3s ease;
}

.gaming-number:hover {
    text-shadow: 0 0 20px rgba(0, 255, 255, 1);
}

/* Gaming icon enhancements */
.gaming-icon {
    transition: all 0.3s ease;
    position: relative;
    overflow: hidden;
}

.gaming-icon::before {
    content: '';
    position: absolute;
    top: -50%;
    left: -50%;
    width: 200%;
    height: 200%;
    background: linear-gradient(45deg, transparent, rgba(255, 255, 255, 0.3), transparent);
    transform: rotate(45deg);
    animation: iconShine 3s infinite;
}

@keyframes iconShine {
    0% { transform: translateX(-100%) translateY(-100%) rotate(45deg); }
    50% { transform: translateX(100%) translateY(100%) rotate(45deg); }
    100% { transform: translateX(-100%) translateY(-100%) rotate(45deg); }
}

/* Gaming action card click feedback */
.gaming-action-card:active {
    transform: scale(0.98);
    transition: transform 0.1s ease;
}

/* Enhanced glitch text animation */
.glitch-text {
    animation: glitchBase 4s infinite;
}

@keyframes glitchBase {
    0%, 90%, 100% { 
        transform: translateX(0);
        filter: hue-rotate(0deg);
    }
    95% { 
        transform: translateX(2px);
        filter: hue-rotate(90deg);
    }
}

/* Gaming dashboard grid enhancement */
.cyber-grid::before {
    content: '';
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    background: 
        radial-gradient(circle at 25% 25%, rgba(0, 255, 136, 0.05) 0%, transparent 50%),
        radial-gradient(circle at 75% 75%, rgba(255, 0, 128, 0.05) 0%, transparent 50%);
    animation: gridPulse 8s ease-in-out infinite;
    z-index: 0;
}

@keyframes gridPulse {
    0%, 100% { opacity: 0.3; }
    50% { opacity: 0.7; }
}

/* Success animation for gaming cards */
@keyframes gamingSuccess {
    0% { 
        transform: scale(1);
        box-shadow: 0 0 0 rgba(16, 185, 129, 0.6);
    }
    50% { 
        transform: scale(1.05);
        box-shadow: 0 0 30px rgba(16, 185, 129, 0.8);
    }
    100% { 
        transform: scale(1);
        box-shadow: 0 0 0 rgba(16, 185, 129, 0.6);
    }
}

.gaming-success {
    animation: gamingSuccess 0.6s ease-out;
}

/* Mobile enhancements */
@media (max-width: 768px) {
    .gaming-action-card {
        padding: 1rem;
    }
    
    .gaming-icon {
        width: 50px;
        height: 50px;
        font-size: 1.2rem;
    }
    
    .gaming-number {
        font-size: 2rem;
    }
    
    .dashboard-fab {
        bottom: 15px;
        right: 15px;
        width: 48px;
        height: 48px;
    }
}

/* Loading spinner for gaming theme */
@keyframes gamingSpinner {
    0% { 
        transform: rotate(0deg);
        border-color: #ff0080 transparent #00ff88 transparent;
    }
    25% { 
        border-color: #00ff88 transparent #00ffff transparent;
    }
    50% { 
        transform: rotate(180deg);
        border-color: #00ffff transparent #ff0080 transparent;
    }
    75% { 
        border-color: #ff0080 transparent #00ff88 transparent;
    }
    100% { 
        transform: rotate(360deg);
        border-color: #ff0080 transparent #00ff88 transparent;
    }
}

.gaming-spinner {
    width: 40px;
    height: 40px;
    border: 3px solid transparent;
    border-radius: 50%;
    animation: gamingSpinner 1s linear infinite;
}
`;

const style = document.createElement('style');
style.textContent = rippleCSS;
document.head.appendChild(style);

// Initialize dashboard when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    window.gadgetHubDashboard = new GadgetHubDashboard();
    
    // Initialize gaming-specific features
    if (document.querySelector('.gaming-dashboard')) {
        console.log('🎮 Gaming Dashboard Mode Activated!');
        initializeGamingFeatures();
    }
});

// Gaming-specific initialization
function initializeGamingFeatures() {
    // Add gaming sound effects (optional)
    const audioContext = window.AudioContext || window.webkitAudioContext;
    
    // Create gaming console messages
    const gamingMessages = [
        "🎮 SYSTEM: Gaming interface loaded successfully",
        "⚡ POWER: All systems operational",
        "🚀 STATUS: Ready for gaming domination",
        "💎 NETWORK: Connected to gaming grid",
        "🔥 ALERT: Gaming mode activated"
    ];
    
    gamingMessages.forEach((msg, index) => {
        setTimeout(() => {
            console.log(`%c${msg}`, 'color: #00ff88; font-weight: bold; text-shadow: 0 0 5px #00ff88;');
        }, index * 500);
    });
    
    // Setup gaming keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        // Ctrl + G = Toggle gaming mode
        if (e.ctrlKey && e.key === 'g') {
            e.preventDefault();
            toggleGamingMode();
        }
        
        // Ctrl + R = Refresh dashboard (enhanced)
        if (e.ctrlKey && e.key === 'r') {
            e.preventDefault();
            if (window.gadgetHubDashboard) {
                window.gadgetHubDashboard.refreshDashboardData();
            }
        }
    });
    
    // Create floating gaming particles
    createFloatingParticles();
}

function toggleGamingMode() {
    const dashboard = document.querySelector('.gaming-dashboard');
    if (dashboard) {
        dashboard.classList.toggle('gaming-enhanced');
        console.log('🎮 Gaming mode toggled!');
    }
}

function createFloatingParticles() {
    setInterval(() => {
        if (Math.random() > 0.8) { // 20% chance
            const particle = document.createElement('div');
            particle.style.cssText = `
                position: fixed;
                width: 4px;
                height: 4px;
                background: #00ff88;
                border-radius: 50%;
                left: ${Math.random() * window.innerWidth}px;
                top: ${window.innerHeight}px;
                pointer-events: none;
                z-index: 1;
                animation: floatUp 4s linear forwards;
            `;
            
            document.body.appendChild(particle);
            
            setTimeout(() => {
                if (particle.parentNode) {
                    particle.remove();
                }
            }, 4000);
        }
    }, 2000);
}

// Add floating particle animation
document.head.insertAdjacentHTML('beforeend', `
<style>
@keyframes floatUp {
    0% {
        opacity: 0;
        transform: translateY(0);
    }
    10% {
        opacity: 1;
    }
    90% {
        opacity: 1;
    }
    100% {
        opacity: 0;
        transform: translateY(-100vh);
    }
}
</style>
`);