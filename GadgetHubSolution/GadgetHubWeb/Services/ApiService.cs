using GadgetHubWeb.Models;
using GadgetHubWeb.Models.DTOs;
using Newtonsoft.Json;
using System.Text;

namespace GadgetHubWeb.Services
{
    public partial class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
            _logger = logger;

            // ✅ REMOVED: Don't set BaseAddress again since it's now set via AddHttpClient
            // The HttpClient is now properly configured in Program.cs
            
            // ✅ ENHANCED: Log the actual HttpClient configuration
            _logger.LogInformation($"🔧 ApiService initialized with base URL: {_baseUrl}");
            _logger.LogInformation($"🔧 HttpClient base address: {_httpClient.BaseAddress}");
            _logger.LogInformation($"🔧 HttpClient timeout: {_httpClient.Timeout}");

            // ✅ ADDED: Set development authentication headers
            var isDevelopment = configuration.GetValue<bool>("Development:EnableTestAuthentication", true);
            if (isDevelopment)
            {
                SetDevelopmentAuthHeaders("Customer", "1", "Test Customer", "test@customer.com");
                _logger.LogInformation($"🔧 Development mode enabled - test headers set");
            }
        }

        // ✅ ADDED: Method to set development authentication headers
        private void SetDevelopmentAuthHeaders(string userType, string userId, string userName, string email)
        {
            _httpClient.DefaultRequestHeaders.Remove("X-User-Type");
            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
            _httpClient.DefaultRequestHeaders.Remove("X-User-Name");
            _httpClient.DefaultRequestHeaders.Remove("X-User-Email");

            _httpClient.DefaultRequestHeaders.Add("X-User-Type", userType);
            _httpClient.DefaultRequestHeaders.Add("X-User-Id", userId);
            _httpClient.DefaultRequestHeaders.Add("X-User-Name", userName);
            _httpClient.DefaultRequestHeaders.Add("X-User-Email", email);

            _logger.LogInformation($"🔧 Development headers set: {userType} {userId} ({userName})");
        }

        // ✅ IMPROVED: Enhanced authentication methods
        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation($"🔑 Auth token set by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        }

        public void SetDevelopmentUser(string userType, string userId, string userName, string email)
        {
            SetDevelopmentAuthHeaders(userType, userId, userName, email);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _logger.LogInformation($"🔓 Auth token cleared by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🔐 Login attempt for: {request.Email} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                _logger.LogInformation($"📡 Making API call to: {_baseUrl}/api/auth/login");

                var response = await _httpClient.PostAsync("api/auth/login", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 API Response Status: {response.StatusCode}");
                _logger.LogInformation($"📄 API Response Body: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseJson);
                    _logger.LogInformation($"✅ Login successful for: {request.Email}");
                    return new ApiResponse<LoginResponse>
                    {
                        Success = true,
                        Data = loginResponse!
                    };
                }

                _logger.LogWarning($"⚠️ Login failed for: {request.Email} - Status: {response.StatusCode}");
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"Login failed: {response.StatusCode}",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in LoginAsync: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Login error - API connection failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> RegisterCustomerAsync(CustomerRegistrationRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🔄 Customer registration API call for: {request.Email} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                var response = await _httpClient.PostAsync("api/auth/register/customer", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Customer registration API Response Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Customer registration API Response Body: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    _logger.LogInformation($"✅ Customer registration successful for: {request.Email}");
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result,
                        Message = "Customer registered successfully"
                    };
                }

                // Handle error responses
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    string errorMessage = errorResponse?.message ?? "Registration failed";
                    
                    _logger.LogWarning($"⚠️ Customer registration failed for {request.Email}: {errorMessage}");
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { errorMessage }
                    };
                }

                _logger.LogWarning($"⚠️ Customer registration failed for {request.Email} - Status: {response.StatusCode}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Registration failed: {response.StatusCode}",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RegisterCustomerAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Registration error - API connection failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> RegisterDistributorAsync(DistributorRegistrationRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🔄 Distributor registration API call for: {request.Email} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                var response = await _httpClient.PostAsync("api/auth/register/distributor", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Distributor registration API Response Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Distributor registration API Response Body: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    _logger.LogInformation($"✅ Distributor registration successful for: {request.Email}");
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result,
                        Message = "Distributor registered successfully"
                    };
                }

                // Handle error responses
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    string errorMessage = errorResponse?.message ?? "Registration failed";
                    
                    _logger.LogWarning($"⚠️ Distributor registration failed for {request.Email}: {errorMessage}");
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { errorMessage }
                    };
                }

                _logger.LogWarning($"⚠️ Distributor registration failed for {request.Email} - Status: {response.StatusCode}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Registration failed: {response.StatusCode}",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RegisterDistributorAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Registration error - API connection failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADDED: Dashboard Methods
        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/dashboard/stats");
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var stats = JsonConvert.DeserializeObject<DashboardStatsDto>(responseJson);
                    return  new ApiResponse<DashboardStatsDto>
                    {
                        Success = true,
                        Data = stats!
                    };
                }

                return new ApiResponse<DashboardStatsDto>
                {
                    Success = false,
                    Message = "Failed to get dashboard stats",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetDashboardStatsAsync: {ex.Message}");
                return new ApiResponse<DashboardStatsDto>
                {
                    Success = false,
                    Message = "Dashboard stats error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADMIN DASHBOARD METHODS
        public async Task<ApiResponse<DashboardStatsDto>> GetAdminDashboardStatsAsync()
        {
            return await GetAsync<DashboardStatsDto>("/api/admin/dashboard/stats");
        }

        public async Task<ApiResponse<AdminOverviewDto>> GetAdminOverviewAsync()
        {
            return await GetAsync<AdminOverviewDto>("/api/admin/overview");
        }

        public async Task<ApiResponse<List<CustomerDto>>> GetAllCustomersAsync()
        {
            return await GetAsync<List<CustomerDto>>("/api/admin/customers");
        }

        public async Task<ApiResponse<List<DistributorDto>>> GetAllDistributorsAsync()
        {
            return await GetAsync<List<DistributorDto>>("/api/admin/distributors");
        }

        public async Task<ApiResponse<List<OrderDto>>> GetAllOrdersAsync()
        {
            return await GetAsync<List<OrderDto>>("/api/admin/orders");
        }

        public async Task<ApiResponse<List<QuotationRequestDto>>> GetAllQuotationRequestsAsync()
        {
            return await GetAsync<List<QuotationRequestDto>>("/api/admin/quotations");
        }

        public async Task<ApiResponse<SystemReportDto>> GetSystemReportsAsync()
        {
            return await GetAsync<SystemReportDto>("/api/admin/reports/system");
        }

        public async Task<ApiResponse<object>> ActivateCustomerAsync(int customerId)
        {
            return await PutAsync<object>($"/api/admin/customers/{customerId}/activate", null);
        }

        public async Task<ApiResponse<object>> DeactivateCustomerAsync(int customerId)
        {
            return await PutAsync<object>($"/api/admin/customers/{customerId}/deactivate", null);
        }

        public async Task<ApiResponse<object>> ActivateDistributorAsync(int distributorId)
        {
            return await PutAsync<object>($"/api/admin/distributors/{distributorId}/activate", null);
        }

        public async Task<ApiResponse<object>> DeactivateDistributorAsync(int distributorId)
        {
            return await PutAsync<object>($"/api/admin/distributors/{distributorId}/deactivate", null);
        }

        // ✅ FIXED: Product Methods
        public async Task<List<ProductDto>> GetProductsAsync(string? searchQuery = null, int? categoryId = null, int page = 1)
        {
            try
            {
                var url = "api/products";
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(searchQuery))
                    queryParams.Add($"search={Uri.EscapeDataString(searchQuery)}");

                if (categoryId.HasValue)
                    queryParams.Add($"categoryId={categoryId}");

                queryParams.Add($"page={page}");

                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                _logger.LogInformation($"🔍 Calling API: {_baseUrl}/{url} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                var response = await _httpClient.GetAsync(url);

                _logger.LogInformation($"📡 API Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"📄 API Response Length: {json.Length} characters");

                    // Parse the response which includes pagination
                    var apiResponse = JsonConvert.DeserializeObject<ApiProductsResponse>(json);

                    var products = apiResponse?.Products?.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Brand = p.Brand,
                        Model = p.Model,
                        ImageUrl = p.ImageUrl,
                        CategoryId = p.Category.Id,
                        CategoryName = p.Category.Name,
                        MinPrice = p.MinPrice,
                        MaxPrice = p.MaxPrice,
                        AvailableStock = p.AvailableStock,
                        DistributorCount = p.DistributorCount,
                        Success = true,
                        Data = p
                    }).ToList() ?? new List<ProductDto>();

                    _logger.LogInformation($"✅ Parsed {products.Count} products by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                    return products;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ API Error: {response.StatusCode} - {errorContent}");
                }

                return new List<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception in GetProductsAsync: {ex.Message}");
                return new List<ProductDto>();
            }
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                _logger.LogInformation($"🔍 Calling API: {_baseUrl}/api/products/categories at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                var response = await _httpClient.GetAsync("api/products/categories");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var categories = JsonConvert.DeserializeObject<List<CategoryDto>>(json) ?? new List<CategoryDto>();

                    // Set response wrapper properties
                    foreach (var category in categories)
                    {
                        category.Success = true;
                        category.Data = category;
                    }

                    _logger.LogInformation($"✅ Parsed {categories.Count} categories");
                    return categories;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ API Error: {response.StatusCode} - {errorContent}");
                }

                return new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception in GetCategoriesAsync: {ex.Message}");
                return new List<CategoryDto>();
            }
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"🔍 Calling API: {_baseUrl}/api/products/{id}");

                var response = await _httpClient.GetAsync($"api/products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiProduct = JsonConvert.DeserializeObject<ApiProductDetail>(json);

                    if (apiProduct != null)
                    {
                        return new ProductDto
                        {
                            Id = apiProduct.Id,
                            Name = apiProduct.Name,
                            Description = apiProduct.Description,
                            Brand = apiProduct.Brand,
                            Model = apiProduct.Model,
                            ImageUrl = apiProduct.ImageUrl,
                            CategoryId = apiProduct.Category.Id,
                            CategoryName = apiProduct.Category.Name,
                            MinPrice = apiProduct.Pricing.Any() ? apiProduct.Pricing.Min(p => p.Price) : 299.99m,
                            MaxPrice = apiProduct.Pricing.Any() ? apiProduct.Pricing.Max(p => p.Price) : 299.99m,
                            DistributorCount = apiProduct.Pricing.Count,
                            Success = true,
                            Data = apiProduct
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception in GetProductByIdAsync: {ex.Message}");
                return null;
            }
        }

        // ✅ ADDED: Cart Methods
        public async Task<ApiResponse<List<CartItemDto>>> GetCartItemsAsync(int customerId = 1)
        {
            try
            {
                _logger.LogInformation($"🔄 ApiService GetCartItemsAsync START: CustomerId={customerId}");
                
                // ✅ ENHANCED: Set development authentication headers
                SetDevelopmentAuthHeaders("Customer", customerId.ToString(), "Test Customer", "customer@test.com");

                _logger.LogInformation($"📤 Getting cart items for customer {customerId}");
                _logger.LogInformation($"🔍 HTTP Client base address: {_httpClient.BaseAddress}");

                var response = await _httpClient.GetAsync($"api/cart?customerId={customerId}");
                
                _logger.LogInformation($"📥 Cart API Response status: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"📥 Cart API Response body: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var cartResponse = JsonConvert.DeserializeObject<CartApiResponse>(responseContent);
                    
                    _logger.LogInformation($"📋 Parsed cart response: Items={cartResponse?.Items?.Count ?? 0}, TotalItems={cartResponse?.TotalItems ?? 0}");

                    if (cartResponse?.Items != null && cartResponse.Items.Any())
                    {
                        foreach (var item in cartResponse.Items)
                        {
                            _logger.LogInformation($"📦 Parsed cart item: ID={item.Id}, ProductId={item.ProductId}, ProductName={item.ProductName}, Quantity={item.Quantity}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ No items found in cart response");
                    }

                    var cartItems = cartResponse?.Items?.Select(item => new CartItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName ?? "Unknown Product",
                        ProductBrand = item.ProductBrand ?? "",
                        ProductImage = item.ProductImage ?? "",
                        Quantity = item.Quantity,
                        Price = item.Price,
                        TotalPrice = item.TotalPrice,
                        EstimatedPrice = item.EstimatedPrice
                    }).ToList() ?? new List<CartItemDto>();

                    _logger.LogInformation($"✅ Retrieved {cartItems.Count} cart items for customer {customerId}");

                    return new ApiResponse<List<CartItemDto>>
                    {
                        Success = true,
                        Data = cartItems,
                        Message = $"Retrieved {cartItems.Count} cart items"
                    };
                }
                else
                {
                    _logger.LogError($"❌ Failed to get cart items: {response.StatusCode} - {responseContent}");
                    
                    return new ApiResponse<List<CartItemDto>>
                    {
                        Success = false,
                        Data = new List<CartItemDto>(),
                        Message = $"Failed to get cart items: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception in GetCartItemsAsync: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                
                return new ApiResponse<List<CartItemDto>>
                {
                    Success = false,
                    Data = new List<CartItemDto>(),
                    Message = $"Error retrieving cart: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<object>> AddToCartAsync(int productId, int quantity = 1, int customerId = 1)
        {
            try
            {
                _logger.LogInformation($"🔄 ApiService AddToCartAsync START: ProductId={productId}, Quantity={quantity}, CustomerId={customerId}");
                
                // ✅ ENHANCED: Set development authentication headers
                SetDevelopmentAuthHeaders("Customer", customerId.ToString(), "Test Customer", "customer@test.com");

                var requestData = new
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    Quantity = quantity
                };

                _logger.LogInformation($"📤 Sending request to API: {JsonConvert.SerializeObject(requestData)}");
                _logger.LogInformation($"🔍 HTTP Client base address: {_httpClient.BaseAddress}");
                _logger.LogInformation($"🔍 HTTP Client headers: {string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/cart/add", content);
                
                _logger.LogInformation($"📥 API Response status: {response.StatusCode}");
                _logger.LogInformation($"📥 API Response headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"📥 API Response content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseContent);
                    _logger.LogInformation($"✅ AddToCart API call successful");
                    
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Product added to cart successfully",
                        Data = result
                    };
                }
                else
                {
                    _logger.LogError($"❌ AddToCart API call failed: {response.StatusCode} - {responseContent}");
                    
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"API Error: {response.StatusCode}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Exception in AddToCartAsync: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error adding to cart: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<object>> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            try
            {
                var request = new { Quantity = quantity };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/cart/{cartItemId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Cart item updated successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update cart item",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in UpdateCartItemAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Update cart error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> UpdateCartItemByProductAsync(int productId, int quantity, int customerId = 1)
        {
            try
            {
                var request = new { CustomerId = customerId, ProductId = productId, Quantity = quantity };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/cart/update-by-product", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Cart item updated successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update cart item",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in UpdateCartItemByProductAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Update cart error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/cart/{cartItemId}");
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Item removed from cart successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to remove from cart",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RemoveFromCartAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Remove from cart error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> RemoveFromCartByProductAsync(int productId, int customerId = 1)
        {
            try
            {
                var request = new { CustomerId = customerId, ProductId = productId };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/cart/remove-by-product", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Item removed from cart successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to remove from cart",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RemoveFromCartByProductAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Remove from cart error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADDED: Order Methods
        public async Task<ApiResponse<List<OrderDto>>> GetCustomerOrdersAsync(int customerId = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/order/customer?customerId={customerId}");
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var orders = JsonConvert.DeserializeObject<List<OrderDto>>(responseJson);
                    return new ApiResponse<List<OrderDto>>
                    {
                        Success = true,
                        Data = orders ?? new List<OrderDto>()
                    };
                }

                return new ApiResponse<List<OrderDto>>
                {
                    Success = false,
                    Message = "Failed to get customer orders",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetCustomerOrdersAsync: {ex.Message}");
                return new ApiResponse<List<OrderDto>>
                {
                    Success = false,
                    Message = "Get orders error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> CreateOrderAsync(object orderRequest)
        {
            try
            {
                var json = JsonConvert.SerializeObject(orderRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/order", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Order created successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create order",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in CreateOrderAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Create order error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADDED: Quotation Methods
        public async Task<ApiResponse<List<QuotationRequestDto>>> GetCustomerQuotationRequestsAsync(int customerId = 1)
        {
            try
            {
                // ✅ ENHANCED: Always use customer ID 1 in development mode
                var actualCustomerId = 1; // Force to use the seeded customer ID
                
                SetDevelopmentUser("Customer", actualCustomerId.ToString(), "Test Customer", "customer@test.com");
                
                var response = await _httpClient.GetAsync($"api/quotation/customer/requests");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"🔄 Customer quotation requests API call - Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Response content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var quotations = JsonConvert.DeserializeObject<List<QuotationRequestDto>>(content) ?? new List<QuotationRequestDto>();
                    _logger.LogInformation($"✅ Retrieved {quotations.Count} quotation requests for customer {actualCustomerId}");
                    
                    return new ApiResponse<List<QuotationRequestDto>>
                    {
                        Success = true,
                        Data = quotations,
                        Message = "Quotation requests retrieved successfully"
                    };
                }

                _logger.LogWarning($"⚠️ Failed to retrieve quotation requests: {response.StatusCode} - {content}");
                return new ApiResponse<List<QuotationRequestDto>>
                {
                    Success = false,
                    Message = $"Failed to retrieve quotation requests: {response.StatusCode}",
                    Data = new List<QuotationRequestDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting customer quotation requests: {ex.Message}");
                return new ApiResponse<List<QuotationRequestDto>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new List<QuotationRequestDto>()
                };
            }
        }

        public async Task<ApiResponse<object>> CreateQuotationRequestAsync(object quotationRequest)
        {
            try
            {
                // ✅ ENHANCED: Ensure customer ID is always 1 for development
                var requestObj = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(quotationRequest));
                if (requestObj != null)
                {
                    requestObj.CustomerId = 1; // Force to use the seeded customer ID
                }
                
                var json = JsonConvert.SerializeObject(requestObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🔄 Sending quotation request to API: {json}");

                // ✅ ENHANCED: Set proper authentication headers
                SetDevelopmentUser("Customer", "1", "Test Customer", "customer@test.com");

                var response = await _httpClient.PostAsync("api/quotation/request", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 API Response: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // Parse the API response to get the message
                    var apiResponse = JsonConvert.DeserializeObject<ApiQuotationResponse>(responseContent);
                    
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = apiResponse?.Message ?? "Quotation request created successfully",
                        Data = apiResponse?.Data
                    };
                }

                // Try to parse error response
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiQuotationResponse>(responseContent);
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? $"Failed to create quotation request: {response.StatusCode}",
                        Data = null
                    };
                }
                catch
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Failed to create quotation request: {response.StatusCode} - {responseContent}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error creating quotation request: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        // ✅ ADDED: Get detailed quotation request with items
        public async Task<ApiResponse<QuotationRequestDetailsDto>> GetQuotationRequestDetailsAsync(int requestId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/quotation/request/{requestId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var requestDetails = JsonConvert.DeserializeObject<QuotationRequestDetailsDto>(content);
                    return new ApiResponse<QuotationRequestDetailsDto>
                    {
                        Success = true,
                        Data = requestDetails,
                        Message = "Quotation request details retrieved successfully"
                    };
                }

                return new ApiResponse<QuotationRequestDetailsDto>
                {
                    Success = false,
                    Message = "Failed to retrieve quotation request details",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting quotation request details: {ex.Message}");
                return new ApiResponse<QuotationRequestDetailsDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<object>> GetQuotationComparisonAsync(int requestId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/quotation/comparison/{requestId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // ✅ FIXED: Use the correct DTO from Models.DTOs namespace
                    var comparison = JsonConvert.DeserializeObject<GadgetHubWeb.Models.DTOs.QuotationComparisonDto>(content);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = comparison,
                        Message = "Quotation comparison retrieved successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to retrieve quotation comparison",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting quotation comparison: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        // ✅ ADDED: Distributor Quotation Methods
        public async Task<ApiResponse<List<QuotationRequestDto>>> GetDistributorQuotationRequestsAsync(int distributorId = 1)
        {
            try
            {
                // ✅ ENHANCED: Set proper development headers for distributor
                SetDevelopmentUser("Distributor", distributorId.ToString(), "TechWorld", "tech@dis.com");
                
                var response = await _httpClient.GetAsync($"api/quotation/distributor/requests");
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"🔄 Distributor quotation requests API call - Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Response content: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    var quotations = JsonConvert.DeserializeObject<List<QuotationRequestDto>>(responseJson);
                    _logger.LogInformation($"✅ Retrieved {quotations?.Count ?? 0} quotation requests for distributor {distributorId}");
                    
                    return new ApiResponse<List<QuotationRequestDto>>
                    {
                        Success = true,
                        Data = quotations ?? new List<QuotationRequestDto>()
                    };
                }

                _logger.LogWarning($"⚠️ Failed to retrieve distributor quotation requests: {response.StatusCode} - {responseJson}");
                return new ApiResponse<List<QuotationRequestDto>>
                {
                    Success = false,
                    Message = "Failed to get distributor quotation requests",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetDistributorQuotationRequestsAsync: {ex.Message}");
                return new ApiResponse<List<QuotationRequestDto>>
                {
                    Success = false,
                    Message = "Get distributor quotations error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADDED: Distributor Order Methods
        public async Task<ApiResponse<List<OrderDto>>> GetDistributorOrdersAsync(int distributorId = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/order/distributor?distributorId={distributorId}");
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var orders = JsonConvert.DeserializeObject<List<OrderDto>>(responseJson);
                    return new ApiResponse<List<OrderDto>>
                    {
                        Success = true,
                        Data = orders ?? new List<OrderDto>()
                    };
                }

                return new ApiResponse<List<OrderDto>>
                {
                    Success = false,
                    Message = "Failed to get distributor orders",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetDistributorOrdersAsync: {ex.Message}");
                return new ApiResponse<List<OrderDto>>
                {
                    Success = false,
                    Message = "Get distributor orders error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADDED: Check if customer has quotation responses available for checkout
        public async Task<ApiResponse<bool>> CanCustomerCheckoutAsync(int customerId = 1)
        {
            try
            {
                var quotationsResponse = await GetCustomerQuotationRequestsAsync(customerId);
                
                if (!quotationsResponse.Success)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to check quotation status",
                        Data = false
                    };
                }

                // Check if customer has at least one quotation request with responses
                bool hasQuotationResponses = quotationsResponse.Data.Any(q => q.HasResponses && q.ResponseCount > 0);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = hasQuotationResponses,
                    Message = hasQuotationResponses ? "Customer can proceed to checkout" : "Customer must request quotations first"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in CanCustomerCheckoutAsync: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error checking checkout eligibility",
                    Data = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADDED: Accept a specific quotation response
        public async Task<ApiResponse<object>> AcceptQuotationResponseAsync(int responseId, int customerId = 1)
        {
            try
            {
                var request = new { ResponseId = responseId, CustomerId = customerId };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/quotation/accept", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Quotation accepted and order created successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to accept quotation",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in AcceptQuotationResponseAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Accept quotation error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADDED: Cancel a quotation request
        public async Task<ApiResponse<object>> CancelQuotationRequestAsync(int requestId, int customerId = 1)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/quotation/cancel/{requestId}", null);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Quotation request cancelled successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to cancel quotation request",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in CancelQuotationRequestAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cancel quotation error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ FIXED: Submit distributor quotation response with enhanced error handling
        public async Task<ApiResponse<object>> SubmitDistributorQuotationResponseAsync(object quotationResponse)
        {
            try
            {
                // ✅ FIXED: Ensure distributor headers are set before making the request
                // The calling code should set these headers before calling this method
                
                var json = JsonConvert.SerializeObject(quotationResponse);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🔄 Sending quotation response to API: {json}");
                _logger.LogInformation($"🔍 Current headers: {string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

                var response = await _httpClient.PostAsync("api/quotation/response", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 API Response: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // Parse the API response to get the message
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiQuotationResponse>(responseContent);
                        
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Message = apiResponse?.Message ?? "Quotation response submitted successfully! Customer will be notified and can compare your offer.",
                            Data = apiResponse?.Data
                        };
                    }
                    catch (JsonException)
                    {
                        // If JSON parsing fails, still return success with default message
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Message = "Quotation response submitted successfully! Customer will be notified and can compare your offer.",
                            Data = null
                        };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning($"⚠️ Forbidden response from API - check authentication headers");
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Access denied. Please ensure you are logged in as a distributor and try again.",
                        Data = null
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning($"⚠️ Bad request from API: {responseContent}");
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        var errorMessage = errorResponse?.message?.ToString() ?? "Invalid request data";
                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = errorMessage,
                            Data = null
                        };
                    }
                    catch
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Invalid request data. Please check your input and try again.",
                            Data = null
                        };
                    }
                }

                // Try to parse error response
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiQuotationResponse>(responseContent);
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? $"Failed to submit quotation response: {response.StatusCode}",
                        Data = null
                    };
                }
                catch
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Failed to submit quotation response: {response.StatusCode} - {responseContent}",
                        Data = null
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"❌ HTTP error submitting quotation response: {httpEx.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Connection error. Please check your internet connection and try again.",
                    Data = null
                };
            }
            catch (TaskCanceledException tcEx)
            {
                _logger.LogError($"❌ Timeout submitting quotation response: {tcEx.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request timed out. Please try again.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error submitting quotation response: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        // ✅ NEW: Get specific quotation response by distributor
        public async Task<ApiResponse<QuotationResponseDto>> GetDistributorQuotationResponseAsync(int requestId, int distributorId = 1)
        {
            try
            {
                SetDevelopmentUser("Distributor", distributorId.ToString(), "TechWorld", "tech@dis.com");
                
                var response = await _httpClient.GetAsync($"api/quotation/distributor/{requestId}/response?distributorId={distributorId}");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"🔄 Getting distributor quotation response - Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Response content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var quotationResponse = JsonConvert.DeserializeObject<QuotationResponseDto>(content);
                    return new ApiResponse<QuotationResponseDto>
                    {
                        Success = true,
                        Data = quotationResponse,
                        Message = "Quotation response retrieved successfully"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<QuotationResponseDto>
                    {
                        Success = false,
                        Message = "No quotation response found for this request",
                        Data = null
                    };
                }

                return new ApiResponse<QuotationResponseDto>
                {
                    Success = false,
                    Message = $"Failed to retrieve quotation response: {response.StatusCode}",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting distributor quotation response: {ex.Message}");
                return new ApiResponse<QuotationResponseDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        // ✅ NEW: Update existing distributor quotation response
        public async Task<ApiResponse<object>> UpdateDistributorQuotationResponseAsync(int responseId, object quotationResponse)
        {
            try
            {
                var json = JsonConvert.SerializeObject(quotationResponse);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🔄 Updating quotation response {responseId}: {json}");

                var response = await _httpClient.PutAsync($"api/quotation/response/{responseId}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Update API Response: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // Parse the API response to get the message
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiQuotationResponse>(responseContent);
                        
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Message = apiResponse?.Message ?? "Quotation response updated successfully!",
                            Data = apiResponse?.Data
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"❌ Failed to parse successful API response: {ex.Message}");
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Data = new { message = "Update completed but response parsing failed" },
                            Message = "Quotation response updated (response format issue)"
                        };
                    }
                }
                else
                {
                    // Enhanced error handling
                    _logger.LogError($"❌ API returned error: {response.StatusCode} - {responseContent}");
                    
                    // Try to parse error details
                    string errorMessage = "Failed to update quotation response";
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (errorResponse?.message != null)
                        {
                            errorMessage = errorResponse.message.ToString();
                        }
                        else if (errorResponse?.error != null)
                        {
                            errorMessage = errorResponse.error.ToString();
                        }
                    }
                    catch
                    {
                        errorMessage = $"HTTP {response.StatusCode}: {responseContent}";
                    }

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { $"HTTP {response.StatusCode}", responseContent }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating quotation response: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        // ✅ NEW: Check if distributor has already responded to a quotation request
        public async Task<ApiResponse<bool>> HasDistributorRespondedAsync(int requestId, int distributorId = 1)
        {
            try
            {
                SetDevelopmentUser("Distributor", distributorId.ToString(), "TechWorld", "tech@dis.com");
                
                var response = await _httpClient.GetAsync($"api/quotation/request/{requestId}/has-response?distributorId={distributorId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<dynamic>(content);
                    bool hasResponded = responseData?.hasResponded ?? false;
                    
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = hasResponded,
                        Message = hasResponded ? "Distributor has already responded" : "Distributor has not responded yet"
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to check response status",
                    Data = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking distributor response status: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }

        // ✅ NEW: Get quotation request items for distributor viewing
        public async Task<ApiResponse<List<QuotationRequestItemDto>>> GetQuotationRequestItemsAsync(int requestId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/quotation/request/{requestId}/items");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"🔄 Getting quotation request items - Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Response content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var items = JsonConvert.DeserializeObject<List<QuotationRequestItemDto>>(content);
                    return new ApiResponse<List<QuotationRequestItemDto>>
                    {
                        Success = true,
                        Data = items ?? new List<QuotationRequestItemDto>(),
                        Message = "Quotation request items retrieved successfully"
                    };
                }

                return new ApiResponse<List<QuotationRequestItemDto>>
                {
                    Success = false,
                    Message = $"Failed to retrieve quotation request items: {response.StatusCode}",
                    Data = new List<QuotationRequestItemDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting quotation request items: {ex.Message}");
                return new ApiResponse<List<QuotationRequestItemDto>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new List<QuotationRequestItemDto>()
                };
            }
        }

        // ✅ ADDED: Distributor Inventory Methods
        public async Task<ApiResponse<List<DistributorInventoryDto>>> GetDistributorInventoryAsync(int distributorId = 1)
        {
            try
            {
                SetDevelopmentUser("Distributor", distributorId.ToString(), "TechWorld", "tech@dis.com");
                
                var response = await _httpClient.GetAsync($"api/distributor/inventory?distributorId={distributorId}");
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"🔄 Distributor inventory API call - Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Response content: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    // ✅ FIXED: Parse the API response structure correctly
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    
                    if (apiResponse?.success == true && apiResponse?.data != null)
                    {
                        // Convert the dynamic data to DistributorInventoryDto list
                        var inventoryItems = new List<DistributorInventoryDto>();
                        
                        foreach (var item in apiResponse.data)
                        {
                            inventoryItems.Add(new DistributorInventoryDto
                            {
                                Id = item.Id,
                                ProductId = item.ProductId,
                                ProductName = item.ProductName ?? "Unknown Product",
                                ProductBrand = item.ProductBrand ?? "Unknown Brand",
                                Category = item.Category ?? "Unknown Category",
                                Price = item.Price,
                                Stock = item.Stock,
                                DeliveryDays = item.DeliveryDays,
                                IsActive = item.IsActive,
                                LastUpdated = DateTime.TryParse(item.LastUpdated?.ToString(), out DateTime lastUpdated) 
                                    ? lastUpdated 
                                    : DateTime.UtcNow
                            });
                        }
                        
                        _logger.LogInformation($"✅ Retrieved {inventoryItems.Count} inventory items for distributor {distributorId}");
                        
                        return new ApiResponse<List<DistributorInventoryDto>>
                        {
                            Success = true,
                            Data = inventoryItems
                        };
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ API returned success but invalid data structure");
                        return new ApiResponse<List<DistributorInventoryDto>>
                        {
                            Success = false,
                            Message = "Invalid API response structure",
                            Data = new List<DistributorInventoryDto>()
                        };
                    }
                }

                _logger.LogWarning($"⚠️ Failed to retrieve distributor inventory: {response.StatusCode} - {responseJson}");
                return new ApiResponse<List<DistributorInventoryDto>>
                {
                    Success = false,
                    Message = "Failed to get distributor inventory",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetDistributorInventoryAsync: {ex.Message}");
                return new ApiResponse<List<DistributorInventoryDto>>
                {
                    Success = false,
                    Message = "Get distributor inventory error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> InitializeDistributorInventoryAsync(int distributorId = 1)
        {
            try
            {
                SetDevelopmentUser("Distributor", distributorId.ToString(), "TechWorld", "tech@dis.com");
                
                var response = await _httpClient.PostAsync($"api/distributor/inventory/initialize?distributorId={distributorId}", null);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"🔄 Initialize distributor inventory API call - Status: {response.StatusCode}");
                _logger.LogInformation($"📄 Response content: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<object>(responseJson);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Data = result!,
                        Message = "Distributor inventory initialized successfully"
                    };
                }

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to initialize distributor inventory",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in InitializeDistributorInventoryAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Initialize distributor inventory error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> UpdateDistributorInventoryItemAsync(int distributorId, int productId, decimal price, int stock, int deliveryDays)
        {
            try
            {
                SetDevelopmentUser("Distributor", distributorId.ToString(), "TechWorld", "tech@dis.com");
                
                var request = new
                {
                    DistributorId = distributorId,
                    Price = price,
                    Stock = stock,
                    DeliveryDays = deliveryDays,
                    IsActive = true
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🔄 Update inventory item API call: URL=api/distributor/inventory/{productId}, Body={json}");

                // ✅ FIXED: Use the correct API endpoint that matches the DistributorController
                var response = await _httpClient.PutAsync($"api/distributor/inventory/{productId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Update inventory API Response: Status={response.StatusCode}, Body={responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<object>(responseJson);
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Data = result!,
                            Message = "Inventory item updated successfully"
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"❌ Failed to parse successful API response: {ex.Message}");
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Data = new { message = "Update completed but response parsing failed" },
                            Message = "Inventory updated (response format issue)"
                        };
                    }
                }
                else
                {
                    _logger.LogError($"❌ API returned error: {response.StatusCode} - {responseJson}");
                    
                    // Try to parse error details
                    string errorMessage = "Failed to update inventory item";
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
                        if (errorResponse?.message != null)
                        {
                            errorMessage = errorResponse.message.ToString();
                        }
                        else if (errorResponse?.error != null)
                        {
                            errorMessage = errorResponse.error.ToString();
                        }
                    }
                    catch
                    {
                        errorMessage = $"HTTP {response.StatusCode}: {responseJson}";
                    }

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { $"HTTP {response.StatusCode}", responseJson }
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"❌ Network error in UpdateDistributorInventoryItemAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Network error - API server may not be running",
                    Errors = new List<string> { "Check if API server is running on the correct port", ex.Message }
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"❌ Timeout error in UpdateDistributorInventoryItemAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request timeout - API server took too long to respond",
                    Errors = new List<string> { "API server timeout", ex.Message }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Unexpected error in UpdateDistributorInventoryItemAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unexpected error occurred",
                    Errors = new List<string> { ex.Message, ex.StackTrace ?? "" }
                };
            }
        }

        // ✅ ADD: Missing generic helper methods
        private async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<T>(responseJson);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = result!
                    };
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Failed to get data from {endpoint}",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in GetAsync<{typeof(T).Name}> for {endpoint}: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Error getting data from {endpoint}",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data)
        {
            try
            {
                HttpContent? content = null;
                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.PutAsync(endpoint, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<T>(responseJson);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = result!
                    };
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Failed to update data at {endpoint}",
                    Errors = new List<string> { responseJson }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in PutAsync<{typeof(T).Name}> for {endpoint}: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Error updating data at {endpoint}",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // ✅ ADD: Missing RemoveFromDistributorInventoryAsync method
        public async Task<ApiResponse<object>> RemoveFromDistributorInventoryAsync(int distributorId, int productId)
        {
            try
            {
                SetDevelopmentUser("Distributor", distributorId.ToString(), "TechWorld", "tech@dis.com");
                
                _logger.LogInformation($"🗑️ Remove from inventory API call: DELETE api/distributor/inventory/{productId}?distributorId={distributorId}");

                var response = await _httpClient.DeleteAsync($"api/distributor/inventory/{productId}?distributorId={distributorId}");
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"📥 Remove from inventory API Response: Status={response.StatusCode}, Body={responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<object>(responseJson);
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Data = result!,
                            Message = "Product removed from inventory successfully"
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"❌ Failed to parse successful API response: {ex.Message}");
                        return new ApiResponse<object>
                        {
                            Success = true,
                            Data = new { message = "Removal completed but response parsing failed" },
                            Message = "Product removed from inventory (response format issue)"
                        };
                    }
                }
                else
                {
                    _logger.LogError($"❌ API returned error: {response.StatusCode} - {responseJson}");
                    
                    // Try to parse error details
                    string errorMessage = "Failed to remove product from inventory";
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
                        if (errorResponse?.message != null)
                        {
                            errorMessage = errorResponse.message.ToString();
                        }
                        else if (errorResponse?.error != null)
                        {
                            errorMessage = errorResponse.error.ToString();
                        }
                    }
                    catch
                    {
                        errorMessage = $"HTTP {response.StatusCode}: {responseJson}";
                    }

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { $"HTTP {response.StatusCode}", responseJson }
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"❌ Network error in RemoveFromDistributorInventoryAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Network error - API server may not be running",
                    Errors = new List<string> { "Check if API server is running on the correct port", ex.Message }
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"❌ Timeout error in RemoveFromDistributorInventoryAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request timeout - API server took too long to respond",
                    Errors = new List<string> { "API server timeout", ex.Message }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Unexpected error in RemoveFromDistributorInventoryAsync: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unexpected error occurred",
                    Errors = new List<string> { ex.Message, ex.StackTrace ?? "" }
                };
            }
        }
    }
}