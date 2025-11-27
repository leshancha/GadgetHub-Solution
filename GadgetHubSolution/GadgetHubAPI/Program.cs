using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using GadgetHubAPI.Data;
using GadgetHubAPI.Services;
using GadgetHubAPI.Authentication;

var builder = WebApplication.CreateBuilder(args);

// ✅ ENHANCED: Smart port configuration with automatic conflict resolution
var (httpPort, httpsPort) = ConfigureSmartPorts(builder);

// Add services to the container.
builder.Services.AddControllers();

// ✅ IMPROVED: Add CORS with specific origins and better configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("GadgetHubWebPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7124", "http://localhost:5047")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
    });
    
    // ✅ ADDED: Development policy for broader access
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ IMPROVED: Add authentication with development bypass option
var jwtKey = builder.Configuration["JWT:Key"];
var developmentMode = builder.Configuration.GetValue<bool>("Development:EnableTestAuthentication");

if (developmentMode)
{
    Console.WriteLine("🔧 Development Mode: Using simplified authentication");
    builder.Services.AddAuthentication("Development")
        .AddScheme<DevelopmentAuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
            "Development", options => { });
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidAudience = builder.Configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""))
            };
        });
}

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDistributorService, DistributorService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Gadget Hub API", 
        Version = "v1",
        Description = "API for Gadget Hub - Electronics B2B Platform"
    });
    
    // Add JWT authentication to Swagger
    if (!developmentMode)
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
});

var app = builder.Build();

// ✅ ADDED: Database initialization with retry logic
await InitializeDatabaseAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gadget Hub API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// ✅ IMPROVED: Add CORS before authentication with environment-specific policy
var corsPolicy = app.Environment.IsDevelopment() ? "DevelopmentPolicy" : "GadgetHubWebPolicy";
app.UseCors(corsPolicy);

Console.WriteLine($"🌐 CORS Policy: {corsPolicy}");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ ENHANCED: Smart startup with port conflict resolution
await StartApplicationSafelyAsync(app, httpPort, httpsPort, developmentMode);

// ✅ NEW: Smart port configuration method
static (int httpPort, int httpsPort) ConfigureSmartPorts(WebApplicationBuilder builder)
{
    var originalHttpPort = 5001;
    var originalHttpsPort = 7001;
    
    Console.WriteLine("🔍 Checking port availability...");
    
    // Check if original ports are available
    var httpPort = IsPortAvailable(originalHttpPort) ? originalHttpPort : FindAvailablePort(5000, 5100);
    var httpsPort = IsPortAvailable(originalHttpsPort) ? originalHttpsPort : FindAvailablePort(7000, 7100);
    
    // Configure the application URLs
    var urls = new List<string>();
    
    if (httpPort != originalHttpPort)
    {
        Console.WriteLine($"⚠️ Original HTTP port {originalHttpPort} is in use, using {httpPort} instead");
    }
    
    if (httpsPort != originalHttpsPort)
    {
        Console.WriteLine($"⚠️ Original HTTPS port {originalHttpsPort} is in use, using {httpsPort} instead");
    }
    
    urls.Add($"http://localhost:{httpPort}");
    urls.Add($"https://localhost:{httpsPort}");
    
    builder.WebHost.UseUrls(urls.ToArray());
    
    Console.WriteLine($"📡 Configured URLs: {string.Join(", ", urls)}");
    
    return (httpPort, httpsPort);
}

// ✅ NEW: Check if a specific port is available
static bool IsPortAvailable(int port)
{
    try
    {
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();
        
        return !tcpConnInfoArray.Any(endpoint => endpoint.Port == port);
    }
    catch
    {
        return false;
    }
}

// ✅ NEW: Find an available port in a range
static int FindAvailablePort(int startPort, int endPort)
{
    for (int port = startPort; port <= endPort; port++)
    {
        if (IsPortAvailable(port))
        {
            return port;
        }
    }
    
    // If no port is available in range, use a random high port
    var random = new Random();
    return random.Next(8000, 9000);
}

// ✅ NEW: Enhanced startup method with comprehensive error handling
static async Task StartApplicationSafelyAsync(WebApplication app, int httpPort, int httpsPort, bool developmentMode)
{
    Console.WriteLine("🚀 Gadget Hub API starting...");
    Console.WriteLine($"👤 API initialized by: leshancha");
    Console.WriteLine($"⏰ Started at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"🔧 Development Mode: {developmentMode}");
    Console.WriteLine($"📡 HTTP URL: http://localhost:{httpPort}");
    Console.WriteLine($"🔒 HTTPS URL: https://localhost:{httpsPort}");
    Console.WriteLine();

    var maxRetries = 3;
    var retryDelay = 2000;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"🔄 Starting application (attempt {attempt}/{maxRetries})...");
            
            // Try to start the application
            app.Run();
            return; // If we get here, the app started successfully
        }
        catch (IOException ex) when (ex.Message.Contains("address already in use") || ex.Message.Contains("Failed to bind"))
        {
            Console.WriteLine();
            Console.WriteLine($"❌ PORT CONFLICT ERROR (Attempt {attempt}/{maxRetries})!");
            Console.WriteLine("===============================================");
            Console.WriteLine($"🚨 Error: {ex.Message}");
            
            if (attempt < maxRetries)
            {
                Console.WriteLine();
                Console.WriteLine("🔄 Attempting automatic port resolution...");
                
                // Try to find new available ports
                var newHttpPort = FindAvailablePort(5100, 5200);
                var newHttpsPort = FindAvailablePort(7100, 7200);
                
                Console.WriteLine($"🔧 Trying new ports: HTTP={newHttpPort}, HTTPS={newHttpsPort}");
                
                // Reconfigure URLs
                var newUrls = new List<string>
                {
                    $"http://localhost:{newHttpPort}",
                    $"https://localhost:{newHttpsPort}"
                };
                
                // This approach won't work after the app is built, so we need to exit and let user restart
                Console.WriteLine();
                Console.WriteLine("⚠️ Port conflict detected. Please use one of these solutions:");
                Console.WriteLine();
                Console.WriteLine("🔧 AUTOMATIC SOLUTIONS:");
                Console.WriteLine("1. Run: .\\fix-port-conflicts.bat");
                Console.WriteLine("2. Run: taskkill /f /im dotnet.exe && .\\start-gadgethub.bat");
                Console.WriteLine();
                Console.WriteLine("🔧 MANUAL SOLUTIONS:");
                Console.WriteLine("3. Close any existing GadgetHub instances");
                Console.WriteLine("4. Stop IIS or other web servers using these ports");
                Console.WriteLine("5. Check Task Manager for dotnet.exe processes");
                Console.WriteLine();
                Console.WriteLine("📋 DIAGNOSTIC COMMANDS:");
                Console.WriteLine($"   netstat -ano | findstr \":{httpsPort}\"");
                Console.WriteLine($"   netstat -ano | findstr \":{httpPort}\"");
                Console.WriteLine("   tasklist | findstr dotnet");
                Console.WriteLine();
                
                await Task.Delay(retryDelay);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("💥 All startup attempts failed due to port conflicts!");
                Console.WriteLine();
                Console.WriteLine("🆘 EMERGENCY SOLUTIONS:");
                Console.WriteLine("1. Restart your computer (clears all port locks)");
                Console.WriteLine("2. Run as Administrator: netsh int ipv4 show excludedportrange protocol=tcp");
                Console.WriteLine("3. Disable Windows reserved ports: netsh int ipv4 set global autotuninglevel=disabled");
                Console.WriteLine();
                Console.WriteLine("⚠️ Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ STARTUP ERROR (Attempt {attempt}/{maxRetries})!");
            Console.WriteLine("===============================================");
            Console.WriteLine($"🚨 Error: {ex.Message}");
            Console.WriteLine($"📍 Stack Trace: {ex.StackTrace}");
            
            if (attempt < maxRetries)
            {
                Console.WriteLine();
                Console.WriteLine($"⏳ Retrying in {retryDelay}ms...");
                await Task.Delay(retryDelay);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("💥 All startup attempts failed!");
                Console.WriteLine("⚠️ Press any key to exit...");
                Console.ReadKey();
                throw;
            }
        }
    }
}

// ✅ ADDED: Database initialization method with proper error handling
static async Task InitializeDatabaseAsync(WebApplication app)
{
    const int maxRetries = 3;
    const int delayBetweenRetriesMs = 2000;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            Console.WriteLine($"🔄 Database initialization attempt {attempt}/{maxRetries}...");
            
            // Ensure database is created and seeded
            await context.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(context);
            await DataSeeder.SeedAsync(context); // This includes users and inventory
            
            Console.WriteLine("✅ Database initialized successfully");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database initialization attempt {attempt} failed: {ex.Message}");
            
            if (attempt == maxRetries)
            {
                Console.WriteLine("💥 All database initialization attempts failed. The application may not work correctly.");
                // Don't throw - let the app start anyway for debugging
            }
            else
            {
                Console.WriteLine($"⏳ Retrying in {delayBetweenRetriesMs}ms...");
                await Task.Delay(delayBetweenRetriesMs);
            }
        }
    }
}