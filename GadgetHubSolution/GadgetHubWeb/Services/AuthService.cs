using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using GadgetHubWeb.Models.DTOs;

namespace GadgetHubWeb.Services
{
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiService _apiService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(IHttpContextAccessor httpContextAccessor, ApiService apiService, ILogger<AuthService> logger, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiService = apiService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> LoginAsync(LoginResponse loginResponse, bool rememberMe = false)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, loginResponse.UserId.ToString()),
                    new(ClaimTypes.Name, loginResponse.Name),
                    new(ClaimTypes.Email, loginResponse.Email),
                    new(ClaimTypes.Role, loginResponse.UserType),
                    new("Token", loginResponse.Token),
                    new("UserType", loginResponse.UserType)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                };

                await _httpContextAccessor.HttpContext!.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                // Store user information in session
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.SetString("AuthToken", loginResponse.Token);
                    httpContext.Session.SetString("UserType", loginResponse.UserType);
                    httpContext.Session.SetString("UserName", loginResponse.Name);
                    httpContext.Session.SetString("UserEmail", loginResponse.Email);
                    httpContext.Session.SetInt32("UserId", loginResponse.UserId);
                }

                // Set API token
                _apiService.SetAuthToken(loginResponse.Token);

                // Set development headers for API communication
                var isDevelopment = _configuration.GetValue<bool>("Development:EnableTestAuthentication", true);
                if (isDevelopment)
                {
                    _apiService.SetDevelopmentUser(loginResponse.UserType, loginResponse.UserId.ToString(), 
                                                   loginResponse.Name, loginResponse.Email);
                }

                _logger.LogInformation("User {Email} logged in successfully at {Timestamp} by leshancha",
                    loginResponse.Email, DateTime.UtcNow);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sign in user {Email} at {Timestamp}",
                    loginResponse.Email, DateTime.UtcNow);
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var userEmail = GetCurrentUserEmail();

                await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Clear API token
                _apiService.ClearAuthToken();

                _logger.LogInformation("User {Email} logged out at {Timestamp} by leshancha",
                    userEmail, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout at {Timestamp}", DateTime.UtcNow);
            }
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public string? GetCurrentUserType()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("UserType")?.Value;
        }

        public string? GetCurrentUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        }

        public string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        public string? GetCurrentUserToken()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("Token")?.Value;
        }

        public bool IsCustomer()
        {
            return GetCurrentUserType() == "Customer";
        }

        public bool IsDistributor()
        {
            return GetCurrentUserType() == "Distributor";
        }

        public bool IsAdmin()
        {
            return GetCurrentUserType() == "Admin";
        }

        public void InitializeApiToken()
        {
            var token = GetCurrentUserToken();
            if (!string.IsNullOrEmpty(token))
            {
                _apiService.SetAuthToken(token);
            }

            // Set development headers if in development mode
            var isDevelopment = _configuration.GetValue<bool>("Development:EnableTestAuthentication", true);
            if (isDevelopment)
            {
                var userType = _httpContextAccessor.HttpContext?.Session.GetString("UserType");
                var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
                var userName = _httpContextAccessor.HttpContext?.Session.GetString("UserName");
                var userEmail = _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");

                if (!string.IsNullOrEmpty(userType) && userId.HasValue)
                {
                    _apiService.SetDevelopmentUser(userType, userId.Value.ToString(), 
                                                   userName ?? "Test User", userEmail ?? "test@example.com");
                }
            }
        }
    }
}