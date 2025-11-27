using Microsoft.AspNetCore.Mvc;
using GadgetHubWeb.Services;
using GadgetHubWeb.Models;
using GadgetHubWeb.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GadgetHubWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApiService apiService, AuthService authService, ILogger<AdminController> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Set admin authentication for API calls
                _apiService.SetAuthToken("admin-token");

                // Get dashboard statistics
                var statsResponse = await _apiService.GetAdminDashboardStatsAsync();
                var overviewResponse = await _apiService.GetAdminOverviewAsync();

                var model = new DashboardViewModel
                {
                    UserName = User.Identity?.Name ?? "Admin",
                    UserId = _authService.GetCurrentUserId() ?? 1,
                    UserType = "Admin"
                };

                if (statsResponse.Success)
                {
                    ViewBag.DashboardStats = statsResponse.Data;
                    ViewBag.TodayOrders = statsResponse.Data?.TodayOrders ?? 0;
                    ViewBag.ThisMonthRevenue = statsResponse.Data?.ThisMonthRevenue ?? 0;
                    ViewBag.PendingQuotations = statsResponse.Data?.PendingQuotations ?? 0;
                    ViewBag.ActiveCustomers = statsResponse.Data?.ActiveCustomers ?? 0;
                    ViewBag.ActiveDistributors = statsResponse.Data?.ActiveDistributors ?? 0;
                }

                if (overviewResponse.Success)
                {
                    ViewBag.TotalCustomers = overviewResponse.Data?.TotalCustomers ?? 0;
                    ViewBag.TotalDistributors = overviewResponse.Data?.TotalDistributors ?? 0;
                    ViewBag.TotalProducts = overviewResponse.Data?.TotalProducts ?? 0;
                    ViewBag.TotalCategories = overviewResponse.Data?.Categories ?? 0;
                }

                _logger.LogInformation($"✅ Admin dashboard loaded for {User.Identity?.Name}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error loading admin dashboard");
                TempData["ErrorMessage"] = "Failed to load dashboard data.";
                return View(new DashboardViewModel { UserName = "Admin", UserId = 1, UserType = "Admin" });
            }
        }

        public async Task<IActionResult> Customers()
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var response = await _apiService.GetAllCustomersAsync();

                var model = new AdminCustomersViewModel
                {
                    Customers = response.Success ? response.Data : new List<CustomerDto>()
                };

                ViewBag.TotalCustomers = model.Customers.Count;
                ViewBag.ActiveCustomers = model.Customers.Count(c => c.IsActive);
                ViewBag.InactiveCustomers = model.Customers.Count(c => !c.IsActive);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error loading customers");
                TempData["ErrorMessage"] = "Failed to load customers.";
                return View(new AdminCustomersViewModel());
            }
        }

        public async Task<IActionResult> Distributors()
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var response = await _apiService.GetAllDistributorsAsync();

                var model = new AdminDistributorsViewModel
                {
                    Distributors = response.Success ? response.Data : new List<DistributorDto>()
                };

                ViewBag.TotalDistributors = model.Distributors.Count;
                ViewBag.ActiveDistributors = model.Distributors.Count(d => d.IsActive);
                ViewBag.InactiveDistributors = model.Distributors.Count(d => !d.IsActive);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error loading distributors");
                TempData["ErrorMessage"] = "Failed to load distributors.";
                return View(new AdminDistributorsViewModel());
            }
        }

        public async Task<IActionResult> Orders()
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var response = await _apiService.GetAllOrdersAsync();

                var model = new AdminOrdersViewModel
                {
                    Orders = response.Success ? response.Data : new List<OrderDto>()
                };

                ViewBag.TotalOrders = model.Orders.Count;
                ViewBag.PendingOrders = model.Orders.Count(o => o.Status == "Pending");
                ViewBag.ProcessingOrders = model.Orders.Count(o => o.Status == "Processing");
                ViewBag.CompletedOrders = model.Orders.Count(o => o.Status == "Delivered");
                ViewBag.TotalRevenue = model.Orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error loading orders");
                TempData["ErrorMessage"] = "Failed to load orders.";
                return View(new AdminOrdersViewModel());
            }
        }

        public async Task<IActionResult> Quotations()
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var response = await _apiService.GetAllQuotationRequestsAsync();

                var model = new AdminQuotationsViewModel
                {
                    QuotationRequests = response.Success ? response.Data : new List<QuotationRequestDto>()
                };

                ViewBag.TotalQuotations = model.QuotationRequests.Count;
                ViewBag.PendingQuotations = model.QuotationRequests.Count(q => q.Status == "Pending");
                ViewBag.CompletedQuotations = model.QuotationRequests.Count(q => q.Status == "Completed");
                ViewBag.ActiveQuotations = model.QuotationRequests.Count(q => q.HasResponses);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error loading quotations");
                TempData["ErrorMessage"] = "Failed to load quotations.";
                return View(new AdminQuotationsViewModel());
            }
        }

        public async Task<IActionResult> Reports()
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var reportResponse = await _apiService.GetSystemReportsAsync();
                var statsResponse = await _apiService.GetAdminDashboardStatsAsync();

                var model = new AdminReportsViewModel
                {
                    SystemReport = reportResponse.Success ? reportResponse.Data : null,
                    DashboardStats = statsResponse.Success ? statsResponse.Data : null,
                    GeneratedAt = DateTime.UtcNow
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error loading reports");
                TempData["ErrorMessage"] = "Failed to load reports.";
                return View(new AdminReportsViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCustomerStatus(int customerId, bool isActive)
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var response = isActive ? 
                    await _apiService.ActivateCustomerAsync(customerId) : 
                    await _apiService.DeactivateCustomerAsync(customerId);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = $"Customer {(isActive ? "activated" : "deactivated")} successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update customer status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error toggling customer status");
                TempData["ErrorMessage"] = "An error occurred while updating customer status.";
            }

            return RedirectToAction("Customers");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDistributorStatus(int distributorId, bool isActive)
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var response = isActive ? 
                    await _apiService.ActivateDistributorAsync(distributorId) : 
                    await _apiService.DeactivateDistributorAsync(distributorId);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = $"Distributor {(isActive ? "activated" : "deactivated")} successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update distributor status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error toggling distributor status");
                TempData["ErrorMessage"] = "An error occurred while updating distributor status.";
            }

            return RedirectToAction("Distributors");
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                var response = await _apiService.GetAdminDashboardStatsAsync();

                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }

                return Json(new { success = false, message = "Failed to load dashboard data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting dashboard data");
                return Json(new { success = false, message = "An error occurred while loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshDashboard()
        {
            try
            {
                // Clear any cache if needed
                await Task.Delay(500); // Simulate refresh time

                TempData["SuccessMessage"] = "Dashboard refreshed successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error refreshing dashboard");
                TempData["ErrorMessage"] = "Failed to refresh dashboard.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductImages()
        {
            try
            {
                _apiService.SetAuthToken("admin-token");
                
                // Call the cart controller's update-product-images endpoint directly
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(
                    "http://localhost:5079/api/cart/update-product-images", 
                    new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                );
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "✅ Product images updated successfully! All products now have reliable placeholder images.";
                    _logger.LogInformation("✅ Admin triggered product image update successfully");
                }
                else
                {
                    TempData["ErrorMessage"] = "❌ Failed to update product images. Please try again.";
                    _logger.LogWarning("⚠️ Product image update failed via admin panel");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating product images");
                TempData["ErrorMessage"] = "❌ An error occurred while updating product images.";
            }

            return RedirectToAction("Index");
        }
    }
}