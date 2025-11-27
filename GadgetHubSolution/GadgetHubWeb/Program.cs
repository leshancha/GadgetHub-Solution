using GadgetHubWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// ✅ ENHANCED: Smart port configuration with automatic conflict resolution
var (httpPort, httpsPort) = ConfigureSmartPorts(builder);

// ✅ CRITICAL FIX: ALL SERVICES MUST BE REGISTERED BEFORE builder.Build()
// =================================================================

// Add MVC and Razor Pages support
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(); // Add Newtonsoft JSON support

builder.Services.AddRazorPages();

// ✅ FIXED: Configure cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// ✅ FIXED: Add HTTP client with proper configuration for ApiService
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001");
    client.DefaultRequestHeaders.Add("User-Agent", "GadgetHub-Web/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    // ✅ ADDED: Allow untrusted certificates for development
    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
});

// ✅ FIX: Add IHttpContextAccessor - THIS WAS MISSING!
builder.Services.AddHttpContextAccessor();

// Add additional HTTP client for generic use
builder.Services.AddHttpClient();

// Add memory cache
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// ✅ CONSOLIDATED: Session configuration (removed duplicate)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ IMPROVED: Add authentication with better configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.Name = "GadgetHubAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                // Check if user session is still valid
                var userToken = context.HttpContext.Session.GetString("UserToken");
                if (string.IsNullOrEmpty(userToken))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync();
                }
            }
        };
    });

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("DistributorOnly", policy => policy.RequireRole("Distributor"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ✅ CONSOLIDATED: Register custom services (removed duplicates)
builder.Services.AddScoped<AuthService>();

// ✅ ENHANCED: Add detailed logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ✅ CRITICAL: Build the app AFTER all services are registered
// ==========================================================
var app = builder.Build();

// ✅ FIXED: Configure the HTTP request pipeline AFTER building
// =============================================================

// Configure exception handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ ENHANCED: Static file serving with image support
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Add cache headers for static files (but images are now external)
        if (ctx.File.Name.EndsWith(".css") || ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
        }
        
        // Log file requests for debugging
        var logger = ctx.Context.RequestServices.GetService<ILogger<Program>>();
        logger?.LogInformation($"📁 Serving static file: {ctx.File.Name}");
    }
});

app.UseRouting();

// Use session before authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add custom routes
app.MapControllerRoute(
    name: "productDetail",
    pattern: "Product/{id:int}",
    defaults: new { controller = "Home", action = "ProductDetail" });

app.MapControllerRoute(
    name: "customerArea",
    pattern: "Customer/{action=Index}",
    defaults: new { controller = "Customer" });

app.MapControllerRoute(
    name: "distributorArea",
    pattern: "Distributor/{action=Index}",
    defaults: new { controller = "Distributor" });

app.MapControllerRoute(
    name: "adminArea",
    pattern: "Admin/{action=Index}",
    defaults: new { controller = "Admin" });

// Map Razor Pages
app.MapRazorPages();

// ✅ ENHANCED: Add startup logging (updated for external URLs only)
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetService<ILogger<Program>>();
    var env = app.Services.GetService<IWebHostEnvironment>();
    
    logger?.LogInformation("🚀 GadgetHub Web Application Started");
    logger?.LogInformation($"📂 Web Root Path: {env?.WebRootPath}");
    logger?.LogInformation($"🌐 Using EXTERNAL URLs ONLY - no local image dependencies");
    logger?.LogInformation($"🚫 Local images directory is no longer required");
    
    // Note about external URLs
    logger?.LogInformation($"✅ All 23 products use high-quality external image URLs");
    logger?.LogInformation($"📈 Images load from official manufacturer sources");
    logger?.LogInformation($"⚡ No local file dependencies - fully cloud-based");
});

Console.WriteLine("🚀 Gadget Hub Web Application starting...");
Console.WriteLine($"👤 Web initialized by: leshancha");
Console.WriteLine($"⏰ Started at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"🌐 HTTP URL: http://localhost:{httpPort}");
Console.WriteLine($"🔒 HTTPS URL: https://localhost:{httpsPort}");
Console.WriteLine($"📡 API URL: {app.Configuration["ApiSettings:BaseUrl"]}");
Console.WriteLine($"🔧 Development mode: {app.Configuration.GetValue<bool>("Development:EnableTestAuthentication", true)}");
Console.WriteLine();

// ✅ FIXED: Simple startup without retry loop to prevent binding conflicts
try
{
    app.Run();
}
catch (IOException ex) when (ex.Message.Contains("address already in use") || ex.Message.Contains("Failed to bind"))
{
    Console.WriteLine();
    Console.WriteLine("❌ PORT CONFLICT ERROR!");
    Console.WriteLine("===============================================");
    Console.WriteLine($"🚨 Error: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("🔧 SOLUTIONS:");
    Console.WriteLine("1. Run: force-stop-and-rebuild.bat");
    Console.WriteLine("2. Close all running GadgetHub instances");
    Console.WriteLine("3. Check Task Manager for dotnet.exe processes");
    Console.WriteLine();
    Console.WriteLine("📋 DIAGNOSTIC COMMANDS:");
    Console.WriteLine($"   netstat -ano | findstr \":{httpsPort}\"");
    Console.WriteLine($"   netstat -ano | findstr \":{httpPort}\"");
    Console.WriteLine("   tasklist | findstr dotnet");
    Console.WriteLine();
    Console.WriteLine("⚠️ Press any key to exit...");
    Console.ReadKey();
    Environment.Exit(1);
}

// ✅ NEW: Smart port configuration method
static (int httpPort, int httpsPort) ConfigureSmartPorts(WebApplicationBuilder builder)
{
    var originalHttpPort = 5047;
    var originalHttpsPort = 7124;
    
    Console.WriteLine("🔍 Checking port availability for GadgetHub Web...");
    
    // Check if original ports are available
    var httpPort = IsPortAvailable(originalHttpPort) ? originalHttpPort : FindAvailablePort(5048, 5148);
    var httpsPort = IsPortAvailable(originalHttpsPort) ? originalHttpsPort : FindAvailablePort(7125, 7225);
    
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
    
    Console.WriteLine($"📡 Configured Web URLs: {string.Join(", ", urls)}");
    
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