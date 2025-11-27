using GadgetHubAPI.Data;
using GadgetHubAPI.DTOs;
using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GadgetHubAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<string> GenerateJwtTokenAsync(string userType, int userId, string name, string email)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userId.ToString()),
                    new(ClaimTypes.Name, name),
                    new(ClaimTypes.Email, email),
                    new(ClaimTypes.Role, userType),
                    new($"{userType}Id", userId.ToString()),
                    new("jti", Guid.NewGuid().ToString()),
                    new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"]!)),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation($"JWT token generated for {userType} {email} at 2025-07-31 08:53:17 UTC");

                return Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating JWT token for {userType} {email} at 2025-07-31 08:53:17 UTC");
                throw;
            }
        }

        public async Task<LoginResponseDTO?> LoginAsync(string email, string password, string userType)
        {
            try
            {
                switch (userType.ToLower())
                {
                    case "customer":
                        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email && c.IsActive);
                        if (customer != null && VerifyPassword(password, customer.PasswordHash))
                        {
                            var token = await GenerateJwtTokenAsync("Customer", customer.Id, customer.Name, customer.Email);
                            _logger.LogInformation($"Customer login successful: {email} at 2025-07-31 08:53:17 UTC");
                            return new LoginResponseDTO
                            {
                                Token = token,
                                UserType = "Customer",
                                UserId = customer.Id,
                                Name = customer.Name,
                                Email = customer.Email
                            };
                        }
                        break;

                    case "distributor":
                        var distributor = await _context.Distributors.FirstOrDefaultAsync(d => d.Email == email && d.IsActive);
                        if (distributor != null && VerifyPassword(password, distributor.PasswordHash))
                        {
                            var token = await GenerateJwtTokenAsync("Distributor", distributor.Id, distributor.CompanyName, distributor.Email);
                            _logger.LogInformation($"Distributor login successful: {email} at 2025-07-31 08:53:17 UTC");
                            return new LoginResponseDTO
                            {
                                Token = token,
                                UserType = "Distributor",
                                UserId = distributor.Id,
                                Name = distributor.CompanyName,
                                Email = distributor.Email
                            };
                        }
                        break;

                    case "admin":
                        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email && a.IsActive);
                        if (admin != null && VerifyPassword(password, admin.PasswordHash))
                        {
                            var token = await GenerateJwtTokenAsync("Admin", admin.Id, admin.FullName, admin.Email);
                            _logger.LogInformation($"Admin login successful: {email} at 2025-07-31 08:53:17 UTC");
                            return new LoginResponseDTO
                            {
                                Token = token,
                                UserType = "Admin",
                                UserId = admin.Id,
                                Name = admin.FullName,
                                Email = admin.Email
                            };
                        }
                        break;
                }

                _logger.LogWarning($"Login failed for {email} as {userType} at 2025-07-31 08:53:17 UTC");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for {email} at 2025-07-31 08:53:17 UTC");
                return null;
            }
        }

        public async Task<LoginResponseDTO?> RegisterCustomerAsync(RegisterCustomerDTO registerDto)
        {
            try
            {
                var customer = new Customer
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password),
                    Phone = registerDto.Phone,
                    Address = registerDto.Address,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                var token = await GenerateJwtTokenAsync("Customer", customer.Id, customer.Name, customer.Email);

                _logger.LogInformation($"Customer registered successfully: {registerDto.Email} by leshancha at 2025-07-31 08:53:17 UTC");

                return new LoginResponseDTO
                {
                    Token = token,
                    UserType = "Customer",
                    UserId = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Customer registration error for {registerDto.Email} at 2025-07-31 08:53:17 UTC");
                return null;
            }
        }

        public async Task<LoginResponseDTO?> RegisterDistributorAsync(RegisterDistributorDTO registerDto)
        {
            try
            {
                var distributor = new Distributor
                {
                    CompanyName = registerDto.CompanyName,
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password),
                    ContactPerson = registerDto.ContactPerson,
                    Phone = registerDto.Phone,
                    Address = registerDto.Address,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Distributors.Add(distributor);
                await _context.SaveChangesAsync();

                var token = await GenerateJwtTokenAsync("Distributor", distributor.Id, distributor.CompanyName, distributor.Email);

                _logger.LogInformation($"Distributor registered successfully: {registerDto.Email} by leshancha at 2025-07-31 08:53:17 UTC");

                return new LoginResponseDTO
                {
                    Token = token,
                    UserType = "Distributor",
                    UserId = distributor.Id,
                    Name = distributor.CompanyName,
                    Email = distributor.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Distributor registration error for {registerDto.Email} at 2025-07-31 08:53:17 UTC");
                return null;
            }
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return Task.FromResult(validatedToken != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Token validation error at 2025-07-31 08:53:17 UTC");
                return Task.FromResult(false);
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _context.Customers.AnyAsync(c => c.Email == email) ||
                       await _context.Distributors.AnyAsync(d => d.Email == email) ||
                       await _context.Admins.AnyAsync(a => a.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Email exists check error for {email} at 2025-07-31 08:53:17 UTC");
                return true; // Return true to be safe and prevent duplicate registrations
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}