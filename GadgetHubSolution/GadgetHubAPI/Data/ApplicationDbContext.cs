using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Distributor> Distributors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<DistributorInventory> DistributorInventories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<QuotationRequest> QuotationRequests { get; set; }
        public DbSet<QuotationRequestItem> QuotationRequestItems { get; set; }
        public DbSet<QuotationResponse> QuotationResponses { get; set; }
        public DbSet<QuotationResponseItem> QuotationResponseItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ ADDED: Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Category)
                      .WithMany(e => e.Products)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure decimal precision for prices - FIXED AMBIGUITY
            modelBuilder.Entity<DistributorInventory>()
                .Property(di => di.Price)
                .HasPrecision(18, 2);

            // QuotationResponseItem price configurations - FULLY QUALIFIED
            modelBuilder.Entity<QuotationResponseItem>()
                .Property(qri => qri.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuotationResponseItem>()
                .Property(qri => qri.TotalPrice)
                .HasPrecision(18, 2);

            // QuotationResponse total price - FULLY QUALIFIED
            modelBuilder.Entity<QuotationResponse>()
                .Property(qr => qr.TotalPrice)
                .HasPrecision(18, 2);

            // OrderItem price configurations - FULLY QUALIFIED
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            // Order total amount - FULLY QUALIFIED
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // Configure relationships - REMOVE DUPLICATE Product CONFIGURATION
            // modelBuilder.Entity<Product>()  // ← REMOVE THIS DUPLICATE
            //     .HasOne(p => p.Category)
            //     .WithMany(c => c.Products)
            //     .HasForeignKey(p => p.CategoryId)
            //     .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DistributorInventory>()
                .HasOne(di => di.Distributor)
                .WithMany(d => d.Inventories)
                .HasForeignKey(di => di.DistributorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DistributorInventory>()
                .HasOne(di => di.Product)
                .WithMany(p => p.DistributorInventories)
                .HasForeignKey(di => di.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Customer)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Distributor)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DistributorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quotation relationships
            modelBuilder.Entity<QuotationRequest>()
                .HasOne(qr => qr.Customer)
                .WithMany(c => c.QuotationRequests)
                .HasForeignKey(qr => qr.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuotationRequestItem>()
                .HasOne(qri => qri.QuotationRequest)
                .WithMany(qr => qr.Items)
                .HasForeignKey(qri => qri.QuotationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuotationRequestItem>()
                .HasOne(qri => qri.Product)
                .WithMany(p => p.QuotationRequestItems)
                .HasForeignKey(qri => qri.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuotationResponse>()
                .HasOne(qr => qr.QuotationRequest)
                .WithMany(qreq => qreq.Responses)
                .HasForeignKey(qr => qr.QuotationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuotationResponse>()
                .HasOne(qr => qr.Distributor)
                .WithMany(d => d.QuotationResponses)
                .HasForeignKey(qr => qr.DistributorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuotationResponseItem>()
                .HasOne(qri => qri.QuotationResponse)
                .WithMany(qr => qr.Items)
                .HasForeignKey(qri => qri.QuotationResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuotationResponseItem>()
                .HasOne(qri => qri.Product)
                .WithMany(p => p.QuotationResponseItems)
                .HasForeignKey(qri => qri.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraints
            modelBuilder.Entity<DistributorInventory>()
                .HasIndex(di => new { di.DistributorId, di.ProductId })
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Distributor>()
                .HasIndex(d => d.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Username)
                .IsUnique();

            // Set default values and timestamps
            modelBuilder.Entity<Product>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // ✅ ADDED: UpdatedAt default value for Product
            modelBuilder.Entity<Product>()
                .Property(p => p.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Customer>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Distributor>()
                .Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Admin>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<QuotationRequest>()
                .Property(qr => qr.RequestDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<QuotationResponse>()
                .Property(qr => qr.SubmissionDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.DateAdded)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<DistributorInventory>()
                .Property(di => di.LastUpdated)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}