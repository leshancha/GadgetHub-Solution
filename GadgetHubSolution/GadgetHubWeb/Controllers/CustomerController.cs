using Microsoft.AspNetCore.Mvc;
using GadgetHubWeb.Services;
using GadgetHubWeb.Models;
using GadgetHubWeb.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace GadgetHubWeb.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ApiService apiService, ILogger<CustomerController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var ordersResponse = await _apiService.GetCustomerOrdersAsync(GetCurrentUserId());
                var quotationsResponse = await _apiService.GetCustomerQuotationRequestsAsync(GetCurrentUserId());

                var orders = ordersResponse.Success ? ordersResponse.Data : new List<OrderDto>();
                var quotations = quotationsResponse.Success ? quotationsResponse.Data : new List<QuotationRequestDto>();

                var viewModel = new DashboardViewModel
                {
                    UserName = User.Identity?.Name ?? "Customer",
                    UserType = "Customer",
                    UserId = GetCurrentUserId(),
                    RecentOrders = orders.Take(5).ToList(),
                    RecentQuotations = quotations.Take(5).ToList()
                };

                // Set ViewBag data for dashboard stats
                ViewBag.TotalOrders = orders.Count;
                ViewBag.TotalSpent = orders.Sum(o => o.TotalAmount);
                ViewBag.PendingQuotations = quotations.Count(q => q.Status == "Pending");
                ViewBag.ActiveQuotations = quotations.Count(q => q.HasResponses);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Customer Index: {ex.Message}");
                var emptyViewModel = new DashboardViewModel
                {
                    UserName = User.Identity?.Name ?? "Customer",
                    UserType = "Customer",
                    UserId = GetCurrentUserId(),
                    RecentOrders = new List<OrderDto>(),
                    RecentQuotations = new List<QuotationRequestDto>()
                };
                return View(emptyViewModel);
            }
        }

        public async Task<IActionResult> Orders()
        {
            try
            {
                var response = await _apiService.GetCustomerOrdersAsync(GetCurrentUserId());
                var orders = response.Success ? response.Data : new List<OrderDto>();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting orders: {ex.Message}");
                return View(new List<OrderDto>());
            }
        }

        public async Task<IActionResult> Quotations()
        {
            try
            {
                var response = await _apiService.GetCustomerQuotationRequestsAsync(GetCurrentUserId());
                var quotations = response.Success ? response.Data : new List<QuotationRequestDto>();
                return View(quotations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting quotations: {ex.Message}");
                return View(new List<QuotationRequestDto>());
            }
        }

        public async Task<IActionResult> Cart()
        {
            try
            {
                var response = await _apiService.GetCartItemsAsync(GetCurrentUserId());
                var cartItems = response.Success ? response.Data : new List<CartItemDto>();
                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting cart: {ex.Message}");
                return View(new List<CartItemDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuotationRequest(CreateQuotationRequestViewModel model)
        {
            try
            {
                var cartResponse = await _apiService.GetCartItemsAsync(GetCurrentUserId());

                if (!cartResponse.Success || !cartResponse.Data.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Add products before creating a quotation request.";
                    return RedirectToAction("Cart");
                }

                var quotationRequest = new
                {
                    CustomerId = GetCurrentUserId(),
                    Items = cartResponse.Data.Select(item => new
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }).ToList()
                };

                var response = await _apiService.CreateQuotationRequestAsync(quotationRequest);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Quotation request created successfully!";
                    return RedirectToAction("Quotations");
                }

                TempData["ErrorMessage"] = response.Message;
                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating quotation: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while creating the quotation request.";
                return RedirectToAction("Cart");
            }
        }

        public async Task<IActionResult> QuotationComparison(int id)
        {
            try
            {
                var response = await _apiService.GetQuotationComparisonAsync(id);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = "Quotation comparison not found or no responses available yet.";
                return RedirectToAction("Quotations");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting quotation comparison: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while getting the quotation comparison.";
                return RedirectToAction("Quotations");
            }
        }

        // Add this new action for better linking from quotations view
        [HttpGet]
        public async Task<IActionResult> CompareQuotations(int requestId)
        {
            return await QuotationComparison(requestId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderViewModel model)
        {
            try
            {
                var cartResponse = await _apiService.GetCartItemsAsync(GetCurrentUserId());

                if (!cartResponse.Success || !cartResponse.Data.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Cart");
                }

                var orderRequest = new
                {
                    CustomerId = GetCurrentUserId(),
                    DistributorId = model.DistributorId,
                    Items = cartResponse.Data.Select(item => new
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.EstimatedPrice
                    }).ToList()
                };

                var response = await _apiService.CreateOrderAsync(orderRequest);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Order created successfully!";
                    return RedirectToAction("Orders");
                }

                TempData["ErrorMessage"] = response.Message;
                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating order: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while creating the order.";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            try
            {
                var response = await _apiService.UpdateCartItemAsync(cartItemId, quantity);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Cart updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }

                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating cart: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating the cart.";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var response = await _apiService.RemoveFromCartAsync(cartItemId);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Item removed from cart successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }

                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error removing from cart: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while removing the item.";
                return RedirectToAction("Cart");
            }
        }

        public async Task<IActionResult> Checkout()
        {
            try
            {
                var cartResponse = await _apiService.GetCartItemsAsync(GetCurrentUserId());

                if (!cartResponse.Success || !cartResponse.Data.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Add products before checkout.";
                    return RedirectToAction("Cart");
                }

                // ✅ NEW: Check if customer has quotation responses before allowing checkout
                var canCheckoutResponse = await _apiService.CanCustomerCheckoutAsync(GetCurrentUserId());
                
                if (!canCheckoutResponse.Success || !canCheckoutResponse.Data)
                {
                    TempData["ErrorMessage"] = "You must request quotations and receive responses from distributors before proceeding to checkout. This ensures you get the best prices!";
                    TempData["InfoMessage"] = "Click 'Request Quotation' in your cart to get competitive prices from multiple distributors.";
                    return RedirectToAction("Cart");
                }

                var cartItems = cartResponse.Data;

                var viewModel = new CheckoutViewModel
                {
                    CartItems = cartItems,
                    TotalAmount = cartItems.Sum(x => x.TotalPrice),
                    DeliveryAddress = User.Identity?.Name ?? "",
                    ContactPhone = ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading checkout: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading checkout.";
                return RedirectToAction("Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RequestQuotation()
        {
            try
            {
                var cartResponse = await _apiService.GetCartItemsAsync(GetCurrentUserId());

                if (!cartResponse.Success || !cartResponse.Data.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Add products before requesting a quotation.";
                    return RedirectToAction("Cart");
                }

                var viewModel = new CreateQuotationRequestViewModel
                {
                    Items = cartResponse.Data.Select(item => new QuotationItemRequest
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Specifications = ""
                    }).ToList(),
                    RequiredDate = DateTime.UtcNow.AddDays(7),
                    DeliveryAddress = User.Identity?.Name ?? "",
                    ContactPhone = ""
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error loading request quotation: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading quotation request.";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RequestQuotation(CreateQuotationRequestViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload cart items for the view
                    var cartResponse = await _apiService.GetCartItemsAsync(GetCurrentUserId());
                    if (cartResponse.Success && cartResponse.Data.Any())
                    {
                        model.Items = cartResponse.Data.Select(item => new QuotationItemRequest
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Specifications = ""
                        }).ToList();
                    }
                    return View(model);
                }

                var cartResponse2 = await _apiService.GetCartItemsAsync(GetCurrentUserId());

                if (!cartResponse2.Success || !cartResponse2.Data.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Add products before requesting a quotation.";
                    return RedirectToAction("Cart", "Home");
                }

                // Create the quotation request with proper data structure
                var quotationRequest = new
                {
                    CustomerId = GetCurrentUserId(),
                    Notes = model.Notes ?? "",
                    RequiredDate = model.RequiredDate,
                    DeliveryAddress = model.DeliveryAddress ?? "",
                    ContactPhone = model.ContactPhone ?? "",
                    Items = cartResponse2.Data.Select(item => new
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Specifications = model.Items.FirstOrDefault(x => x.ProductId == item.ProductId)?.Specifications ?? ""
                    }).ToList()
                };

                _logger.LogInformation($"🔄 Creating quotation request for customer {GetCurrentUserId()} with {quotationRequest.Items.Count()} items at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                var response = await _apiService.CreateQuotationRequestAsync(quotationRequest);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = response.Message ?? "Quotation request submitted successfully! Distributors will respond within 24-48 hours. You'll be notified when responses are available.";
                    
                    // Clear the cart after successful quotation request
                    try
                    {
                        foreach (var item in cartResponse2.Data)
                        {
                            await _apiService.RemoveFromCartAsync(item.Id);
                        }
                        _logger.LogInformation($"✅ Cart cleared after quotation request for customer {GetCurrentUserId()}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clear cart after quotation request");
                    }
                    
                    return RedirectToAction("Quotations");
                }

                TempData["ErrorMessage"] = response.Message ?? "Failed to create quotation request. Please try again.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating quotation request: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while creating the quotation request. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AcceptQuotation(int responseId)
        {
            try
            {
                var response = await _apiService.AcceptQuotationResponseAsync(responseId, GetCurrentUserId());

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Quotation accepted successfully! Your order has been created.";
                    return RedirectToAction("Orders");
                }

                TempData["ErrorMessage"] = response.Message;
                return RedirectToAction("Quotations");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error accepting quotation: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while accepting the quotation.";
                return RedirectToAction("Quotations");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelQuotation(int id)
        {
            try
            {
                // Add API service method for cancelling quotations
                var response = await _apiService.CancelQuotationRequestAsync(id, GetCurrentUserId());

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Quotation request cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }

                return RedirectToAction("Quotations");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error cancelling quotation: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while cancelling the quotation request.";
                return RedirectToAction("Quotations");
            }
        }

        private int GetCurrentUserId()
        {
            // ✅ IMPROVED: Better user ID retrieval with development fallback
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
            {
                // ✅ NEW: In development mode, always use customer ID 1 for consistency
                var isDevelopment = HttpContext.RequestServices.GetService<IConfiguration>()
                    ?.GetValue<bool>("Development:EnableTestAuthentication", true) ?? false;
                
                if (isDevelopment)
                {
                    _logger.LogInformation($"🔧 Development mode: Using customer ID 1 instead of session ID {sessionUserId.Value}");
                    return 1; // Always use the seeded test customer ID
                }
                
                return sessionUserId.Value;
            }

            // ✅ ADDED: Development mode fallback
            var isDevelopment2 = HttpContext.RequestServices.GetService<IConfiguration>()
                ?.GetValue<bool>("Development:EnableTestAuthentication", true) ?? false;
            
            if (isDevelopment2)
            {
                _logger.LogInformation("🔧 Using development user ID: 1");
                return 1; // Default test customer ID
            }

            _logger.LogWarning("⚠️ No user ID found in session and not in development mode");
            return 1; // Fallback
        }
    }
}