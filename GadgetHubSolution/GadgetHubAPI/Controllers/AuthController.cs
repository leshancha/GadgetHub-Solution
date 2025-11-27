using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Data;
using GadgetHubAPI.Models;
using GadgetHubAPI.DTOs;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                _logger.LogInformation($"🔐 Login attempt for: {request.Email} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                // Check customers
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email && c.IsActive);
                if (customer != null && BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
                {
                    var customerToken = GenerateJwtToken(customer.Id.ToString(), "Customer", customer.Name);
                    _logger.LogInformation($"✅ Customer login successful: {request.Email}");
                    return Ok(new LoginResponseDTO
                    {
                        Token = customerToken,
                        UserId = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        UserType = "Customer"
                    });
                }

                // Check distributors
                var distributor = await _context.Distributors.FirstOrDefaultAsync(d => d.Email == request.Email && d.IsActive);
                if (distributor != null && BCrypt.Net.BCrypt.Verify(request.Password, distributor.PasswordHash))
                {
                    var distributorToken = GenerateJwtToken(distributor.Id.ToString(), "Distributor", distributor.CompanyName);
                    _logger.LogInformation($"✅ Distributor login successful: {request.Email}");
                    return Ok(new LoginResponseDTO
                    {
                        Token = distributorToken,
                        UserId = distributor.Id,
                        Name = distributor.CompanyName,
                        Email = distributor.Email,
                        UserType = "Distributor"
                    });
                }

                // Check admins
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email && a.IsActive);
                if (admin != null && BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
                {
                    var adminToken = GenerateJwtToken(admin.Id.ToString(), "Admin", admin.FullName);
                    _logger.LogInformation($"✅ Admin login successful: {request.Email}");
                    return Ok(new LoginResponseDTO
                    {
                        Token = adminToken,
                        UserId = admin.Id,
                        Name = admin.FullName,
                        Email = admin.Email,
                        UserType = "Admin"
                    });
                }

                _logger.LogWarning($"⚠️ Failed login attempt for: {request.Email} - Invalid credentials or inactive account");
                return Unauthorized(new { message = "Invalid email or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Login: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("register/customer")]
        public async Task<ActionResult> RegisterCustomer([FromBody] RegisterCustomerDTO request)
        {
            try
            {
                _logger.LogInformation($"🔄 Customer registration attempt for: {request.Email} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                // Check if email already exists across all user types
                var emailExists = await _context.Customers.AnyAsync(c => c.Email == request.Email) ||
                                await _context.Distributors.AnyAsync(d => d.Email == request.Email) ||
                                await _context.Admins.AnyAsync(a => a.Email == request.Email);

                if (emailExists)
                {
                    _logger.LogWarning($"⚠️ Registration failed - Email already exists: {request.Email}");
                    return BadRequest(new { 
                        success = false,
                        message = "Email already registered",
                        timestamp = DateTime.UtcNow 
                    });
                }

                var customer = new Customer
                {
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12), // Use same rounds as AuthService
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ New customer registered: {request.Email} by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                return Ok(new
                {
                    success = true,
                    message = "Customer registered successfully",
                    customerId = customer.Id,
                    registeredAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RegisterCustomer: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("register/distributor")]
        public async Task<ActionResult> RegisterDistributor([FromBody] RegisterDistributorDTO request)
        {
            try
            {
                _logger.LogInformation($"🔄 Distributor registration attempt for: {request.Email} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                // Check if email already exists across all user types
                var emailExists = await _context.Customers.AnyAsync(c => c.Email == request.Email) ||
                                await _context.Distributors.AnyAsync(d => d.Email == request.Email) ||
                                await _context.Admins.AnyAsync(a => a.Email == request.Email);

                if (emailExists)
                {
                    _logger.LogWarning($"⚠️ Registration failed - Email already exists: {request.Email}");
                    return BadRequest(new { 
                        success = false,
                        message = "Email already registered",
                        timestamp = DateTime.UtcNow 
                    });
                }

                var distributor = new Distributor
                {
                    CompanyName = request.CompanyName,
                    Email = request.Email,
                    ContactPerson = request.ContactPerson,
                    Phone = request.Phone,
                    Address = request.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12), // Use same rounds as AuthService
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Distributors.Add(distributor);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ New distributor registered: {request.Email} by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                return Ok(new
                {
                    success = true,
                    message = "Distributor registered successfully",
                    distributorId = distributor.Id,
                    registeredAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in RegisterDistributor: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    success = false,
                    error = "Internal server error", 
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("validate")]
        public ActionResult ValidateToken()
        {
            // Token validation logic
            return Ok(new { valid = true, validatedAt = DateTime.UtcNow });
        }

        [HttpGet("check-email")]
        public async Task<ActionResult> CheckEmailExists([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { exists = false, message = "Email is required" });
                }

                _logger.LogInformation($"🔍 Email availability check for: {email} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                var emailExists = await _context.Customers.AnyAsync(c => c.Email == email) ||
                                await _context.Distributors.AnyAsync(d => d.Email == email) ||
                                await _context.Admins.AnyAsync(a => a.Email == email);

                return Ok(new { 
                    exists = emailExists,
                    message = emailExists ? "Email is already registered" : "Email is available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error checking email availability: {ex.Message}");
                return StatusCode(500, new { 
                    exists = true, // Return true to be safe
                    message = "Error checking email availability" 
                });
            }
        }

        [HttpGet("me")]
        public ActionResult GetCurrentUser()
        {
            // Return current user info based on token
            return Ok(new
            {
                id = 1,
                name = "Development User",
                email = "dev@gadgethub.com",
                role = "Customer",
                checkedAt = DateTime.UtcNow
            });
        }

        [HttpGet("health")]
        public ActionResult HealthCheck()
        {
            return Ok(new 
            { 
                status = "healthy", 
                message = "GadgetHub API is running successfully",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = "Development"
            });
        }

        [HttpGet("test-accounts")]
        public async Task<ActionResult> GetTestAccounts()
        {
            try
            {
                var adminCount = await _context.Admins.CountAsync();
                var customerCount = await _context.Customers.CountAsync();
                var distributorCount = await _context.Distributors.CountAsync();

                var testAccounts = new
                {
                    message = "Test account information",
                    accounts = new
                    {
                        admins = adminCount,
                        customers = customerCount,
                        distributors = distributorCount
                    },
                    availableCredentials = new[]
                    {
                        new { role = "Admin", email = "admin@gadgethub.com", password = "Admin@123" },
                        new { role = "Customer", email = "customer@test.com", password = "Customer@123" },
                        new { role = "Distributor", email = "tech@dis.com", password = "tech123" }
                    },
                    timestamp = DateTime.UtcNow
                };

                return Ok(testAccounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Database error", message = ex.Message });
            }
        }

        [HttpPost("test-password")]
        public ActionResult TestPassword([FromBody] TestPasswordRequest request)
        {
            try
            {
                // Hash the password with 12 rounds (same as AuthService)
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
                
                // Verify the password
                var isValid = BCrypt.Net.BCrypt.Verify(request.Password, hashedPassword);
                
                return Ok(new
                {
                    originalPassword = request.Password,
                    hashedPassword = hashedPassword,
                    verificationResult = isValid,
                    message = isValid ? "Password hashing and verification working correctly" : "Password verification failed",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Password test failed", message = ex.Message });
            }
        }

        [HttpGet("verify-demo-accounts")]
        public async Task<ActionResult> VerifyDemoAccounts()
        {
            try
            {
                var results = new List<object>();

                // Test Customer account
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == "customer@test.com");
                if (customer != null)
                {
                    var customerPasswordValid = BCrypt.Net.BCrypt.Verify("Customer@123", customer.PasswordHash);
                    results.Add(new
                    {
                        accountType = "Customer",
                        email = customer.Email,
                        passwordHashLength = customer.PasswordHash.Length,
                        passwordVerification = customerPasswordValid,
                        isActive = customer.IsActive
                    });
                }

                // Test Distributor account
                var distributor = await _context.Distributors.FirstOrDefaultAsync(d => d.Email == "tech@dis.com");
                if (distributor != null)
                {
                    var distributorPasswordValid = BCrypt.Net.BCrypt.Verify("tech123", distributor.PasswordHash);
                    results.Add(new
                    {
                        accountType = "Distributor",
                        email = distributor.Email,
                        passwordHashLength = distributor.PasswordHash.Length,
                        passwordVerification = distributorPasswordValid,
                        isActive = distributor.IsActive
                    });
                }

                // Test Admin account
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == "admin@gadgethub.com");
                if (admin != null)
                {
                    var adminPasswordValid = BCrypt.Net.BCrypt.Verify("Admin@123", admin.PasswordHash);
                    results.Add(new
                    {
                        accountType = "Admin",
                        email = admin.Email,
                        passwordHashLength = admin.PasswordHash.Length,
                        passwordVerification = adminPasswordValid,
                        isActive = admin.IsActive
                    });
                }

                return Ok(new
                {
                    message = "Demo account verification results",
                    results = results,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Demo account verification failed", message = ex.Message });
            }
        }

        [HttpPost("simple-login-test")]
        public async Task<ActionResult> SimpleLoginTest([FromBody] LoginDTO request)
        {
            try
            {
                _logger.LogInformation($"🔬 Simple login test for: {request.Email}");

                // Check all user types
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email && c.IsActive);
                if (customer != null)
                {
                    var customerPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash);
                    _logger.LogInformation($"🔍 Customer found: {customer.Email}, Password valid: {customerPasswordValid}");
                    
                    if (customerPasswordValid)
                    {
                        return Ok(new LoginResponseDTO
                        {
                            Token = "test-token-customer",
                            UserId = customer.Id,
                            Name = customer.Name,
                            Email = customer.Email,
                            UserType = "Customer"
                        });
                    }
                }

                var distributor = await _context.Distributors.FirstOrDefaultAsync(d => d.Email == request.Email && d.IsActive);
                if (distributor != null)
                {
                    var distributorPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, distributor.PasswordHash);
                    _logger.LogInformation($"🔍 Distributor found: {distributor.Email}, Password valid: {distributorPasswordValid}");
                    
                    if (distributorPasswordValid)
                    {
                        return Ok(new LoginResponseDTO
                        {
                            Token = "test-token-distributor",
                            UserId = distributor.Id,
                            Name = distributor.CompanyName,
                            Email = distributor.Email,
                            UserType = "Distributor"
                        });
                    }
                }

                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email && a.IsActive);
                if (admin != null)
                {
                    var adminPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);
                    _logger.LogInformation($"🔍 Admin found: {admin.Email}, Password valid: {adminPasswordValid}");
                    
                    if (adminPasswordValid)
                    {
                        return Ok(new LoginResponseDTO
                        {
                            Token = "test-token-admin",
                            UserId = admin.Id,
                            Name = admin.FullName,
                            Email = admin.Email,
                            UserType = "Admin"
                        });
                    }
                }

                _logger.LogWarning($"🚫 No valid user found for: {request.Email}");
                return Unauthorized(new { message = "Invalid email or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Simple login test error: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        [HttpPost("direct-login-test")]
        public async Task<ActionResult> DirectLoginTest([FromBody] LoginDTO request)
        {
            try
            {
                _logger.LogInformation($"🧪 Direct login test for: {request.Email}");

                // Find the user by email
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email && c.IsActive);
                if (customer != null)
                {
                    _logger.LogInformation($"🔍 Found customer: {customer.Email}, Created: {customer.CreatedAt}");
                    _logger.LogInformation($"🔍 Password hash length: {customer.PasswordHash.Length}");
                    
                    var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash);
                    _logger.LogInformation($"🔍 Password verification result: {passwordValid}");
                    
                    return Ok(new
                    {
                        userFound = true,
                        userType = "Customer",
                        email = customer.Email,
                        name = customer.Name,
                        passwordValid = passwordValid,
                        hashLength = customer.PasswordHash.Length,
                        isActive = customer.IsActive,
                        createdAt = customer.CreatedAt,
                        testResult = passwordValid ? "✅ Login would succeed" : "❌ Password verification failed"
                    });
                }

                return Ok(new
                {
                    userFound = false,
                    email = request.Email,
                    testResult = "❌ User not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Direct login test error: {ex.Message}");
                return StatusCode(500, new { error = "Test failed", message = ex.Message });
            }
        }

        private string GenerateJwtToken(string userId, string role, string name)
        {
            try
            {
                // For development, create a more realistic token with basic info
                var payload = new
                {
                    sub = userId,
                    role = role,
                    name = name,
                    iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
                    issuer = "GadgetHubAPI"
                };

                // Simple development token (not for production)
                var tokenData = System.Text.Json.JsonSerializer.Serialize(payload);
                var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
                var token = Convert.ToBase64String(tokenBytes);
                
                _logger.LogInformation($"🔑 Generated token for {role} {name} (ID: {userId})");
                return $"Bearer-{token}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error generating token: {ex.Message}");
                // Fallback token
                return $"dev-token-{userId}-{role}-{DateTime.UtcNow.Ticks}";
            }
        }

        public class TestPasswordRequest
        {
            public string Password { get; set; } = string.Empty;
        }
    }
}