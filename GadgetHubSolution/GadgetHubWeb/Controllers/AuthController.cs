using GadgetHubWeb.Models;
using GadgetHubWeb.Models.DTOs;
using GadgetHubWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace GadgetHubWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApiService apiService, ILogger<AuthController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            try
            {
                // Debug: Log ModelState validation details
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    
                    _logger.LogWarning($"⚠️ ModelState validation failed for {viewModel.Email}. Errors: {string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}"))}");
                    
                    // Let's proceed anyway if we have email and password, but log the validation issue
                    if (string.IsNullOrEmpty(viewModel.Email) || string.IsNullOrEmpty(viewModel.Password))
                    {
                        TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                        return View(viewModel);
                    }
                    
                    // Clear ModelState to proceed with login attempt
                    ModelState.Clear();
                }

                _logger.LogInformation($"🔄 Processing login for: {viewModel.Email}");

                // Convert ViewModel to DTO for API call
                var loginRequest = new LoginRequest
                {
                    Email = viewModel.Email,
                    Password = viewModel.Password
                };

                var response = await _apiService.LoginAsync(loginRequest);

                if (response.Success)
                {
                    // Set auth token for API calls
                    _apiService.SetAuthToken(response.Data.Token);

                    // Create claims for the user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, response.Data.UserId.ToString()),
                        new Claim(ClaimTypes.Name, response.Data.Name),
                        new Claim(ClaimTypes.Email, response.Data.Email),
                        new Claim(ClaimTypes.Role, response.Data.UserType)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = viewModel.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Store additional info in session
                    HttpContext.Session.SetString("UserToken", response.Data.Token);
                    HttpContext.Session.SetString("UserName", response.Data.Name);
                    HttpContext.Session.SetString("UserRole", response.Data.UserType);
                    HttpContext.Session.SetInt32("UserId", response.Data.UserId);

                    _logger.LogInformation($"✅ User {viewModel.Email} logged in successfully by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                    // Check for return URL
                    if (!string.IsNullOrEmpty(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
                    {
                        return Redirect(viewModel.ReturnUrl);
                    }

                    // Redirect based on role
                    return response.Data.UserType switch
                    {
                        "Admin" => RedirectToAction("Index", "Admin"),
                        "Distributor" => RedirectToAction("Index", "Distributor"),
                        _ => RedirectToAction("Index", "Customer")
                    };
                }

                // Handle specific error cases
                string errorMessage;
                if (response.Errors?.Any() == true)
                {
                    var errorContent = string.Join(", ", response.Errors);
                    if (errorContent.Contains("Invalid email or password") || 
                        errorContent.Contains("Unauthorized") || 
                        errorContent.Contains("401"))
                    {
                        errorMessage = "❌ Invalid email or password. Please check your credentials and try again.";
                    }
                    else if (errorContent.Contains("inactive account"))
                    {
                        errorMessage = "❌ Your account has been deactivated. Please contact support.";
                    }
                    else if (errorContent.Contains("Internal server error") || 
                             errorContent.Contains("500"))
                    {
                        errorMessage = "❌ Server error occurred. Please try again later or contact support.";
                    }
                    else
                    {
                        errorMessage = $"❌ Login failed: {errorContent}";
                    }
                }
                else
                {
                    errorMessage = "❌ Invalid email or password. Please check your credentials and try again.";
                }

                _logger.LogWarning($"⚠️ Login failed for {viewModel.Email}: {errorMessage}");
                TempData["ErrorMessage"] = errorMessage;
                return View(viewModel);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"❌ API connection error: {httpEx.Message}");
                TempData["ErrorMessage"] = "❌ Unable to connect to the server. Please ensure both applications are running and try again.";
                return View(viewModel);
            }
            catch (TaskCanceledException tcEx)
            {
                _logger.LogError($"❌ Login timeout: {tcEx.Message}");
                TempData["ErrorMessage"] = "❌ Login request timed out. Please check your connection and try again.";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Login error: {ex.Message}");
                TempData["ErrorMessage"] = "❌ An unexpected error occurred during login. Please try again.";
                return View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult RegisterCustomer()
        {
            return View(new RegisterCustomerViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCustomer(RegisterCustomerViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                _logger.LogInformation($"🔄 Processing customer registration for: {model.Email}");

                // Map ViewModel to API DTO
                var registrationRequest = new CustomerRegistrationRequest
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    Phone = model.Phone,
                    Address = model.Address
                };

                var response = await _apiService.RegisterCustomerAsync(registrationRequest);

                if (response.Success)
                {
                    _logger.LogInformation($"✅ Customer registration successful for: {model.Email}");
                    TempData["SuccessMessage"] = "✅ Registration successful! Please login with your credentials.";
                    return RedirectToAction("Login");
                }

                // Handle registration errors
                _logger.LogWarning($"⚠️ Customer registration failed for {model.Email}: {response.Message}");
                
                if (response.Message?.Contains("Email already registered") == true)
                {
                    ModelState.AddModelError("Email", "This email address is already registered. Please use a different email or try logging in.");
                }
                else
                {
                    ModelState.AddModelError("", response.Message ?? "Registration failed. Please try again.");
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Customer registration error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult RegisterDistributor()
        {
            return View(new RegisterDistributorViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDistributor(RegisterDistributorViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Map ViewModel to API DTO
                var registrationRequest = new DistributorRegistrationRequest
                {
                    CompanyName = model.CompanyName,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    ContactPerson = model.ContactPerson,
                    Phone = model.Phone,
                    Address = model.Address
                };

                _logger.LogInformation($"🔄 Processing distributor registration for: {model.Email}");

                var response = await _apiService.RegisterDistributorAsync(registrationRequest);

                if (response.Success)
                {
                    _logger.LogInformation($"✅ Distributor registration successful for: {model.Email}");
                    TempData["SuccessMessage"] = "✅ Registration successful! Please login with your credentials.";
                    return RedirectToAction("Login");
                }

                // Handle registration errors
                _logger.LogWarning($"⚠️ Distributor registration failed for {model.Email}: {response.Message}");
                
                if (response.Message?.Contains("Email already registered") == true)
                {
                    ModelState.AddModelError("Email", "This email address is already registered. Please use a different email or try logging in.");
                }
                else
                {
                    ModelState.AddModelError("", response.Message ?? "Registration failed. Please try again.");
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Distributor registration error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            _apiService.ClearAuthToken();
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation($"✅ User logged out by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Route("/api/auth/check-email")]
        public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { exists = false });
                }

                // ✅ TEMPORARILY DISABLED: Make a call to the API to check email availability
                // var response = await _apiService.CheckEmailExistsAsync(email);
                
                // ✅ TEMPORARY: Mock response
                await Task.Delay(100);
                
                return Json(new { 
                    exists = false // Assume email is available for now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking email availability: {ex.Message}");
                return Json(new { exists = false }); // Assume available on error
            }
        }
    }
}