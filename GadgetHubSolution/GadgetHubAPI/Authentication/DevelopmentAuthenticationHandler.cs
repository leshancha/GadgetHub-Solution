using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GadgetHubAPI.Authentication
{
    public class DevelopmentAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        // ✅ FIXED: Use new keyword to suppress hiding warning
        public new TimeProvider TimeProvider { get; set; } = TimeProvider.System;
    }

    public class DevelopmentAuthenticationHandler : AuthenticationHandler<DevelopmentAuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        // ✅ FIXED: Remove obsolete ISystemClock parameter and use TimeProvider instead
        public DevelopmentAuthenticationHandler(
            IOptionsMonitor<DevelopmentAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // ✅ IMPROVED: Get user info from headers or use defaults
            var userType = Request.Headers["X-User-Type"].FirstOrDefault() ?? 
                           _configuration["Development:DefaultTestUserType"] ?? "Customer";
            
            var userId = Request.Headers["X-User-Id"].FirstOrDefault() ?? 
                         _configuration["Development:DefaultTestUserId"] ?? "1";

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? 
                           (userType == "Customer" ? "Test Customer" : 
                            userType == "Distributor" ? "Test Distributor" : "Test Admin");

            var email = Request.Headers["X-User-Email"].FirstOrDefault() ?? 
                        (userType == "Customer" ? "test@customer.com" : 
                         userType == "Distributor" ? "test@distributor.com" : "test@admin.com");

            // Create claims based on user type
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, userType),
                new Claim("UserId", userId),
                new Claim("UserType", userType)
            };

            // Add type-specific claims
            switch (userType.ToLower())
            {
                case "customer":
                    claims.Add(new Claim("CustomerId", userId));
                    break;
                case "distributor":
                    claims.Add(new Claim("DistributorId", userId));
                    break;
                case "admin":
                    claims.Add(new Claim("AdminId", userId));
                    break;
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation($"🔧 Development Auth: {userType} {userId} ({userName}) authenticated");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}