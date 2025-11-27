using GadgetHubAPI.Data;
using GadgetHubAPI.DTOs;
using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProductDTO>> GetAllProductsAsync()
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        ImageUrl = p.ImageUrl,
                        Brand = p.Brand,
                        Model = p.Model,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all products at 2025-07-31 08:48:25 UTC");
                return new List<ProductDTO>();
            }
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Id == id)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        ImageUrl = p.ImageUrl,
                        Brand = p.Brand,
                        Model = p.Model,
                        CreatedAt = p.CreatedAt
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {id} at 2025-07-31 08:48:25 UTC");
                return null;
            }
        }

        public async Task<List<ProductDTO>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllProductsAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                return await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Name.ToLower().Contains(lowerSearchTerm) ||
                               (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)) ||
                               (p.Brand != null && p.Brand.ToLower().Contains(lowerSearchTerm)) ||
                               (p.Model != null && p.Model.ToLower().Contains(lowerSearchTerm)) ||
                               p.Category.Name.ToLower().Contains(lowerSearchTerm))
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        ImageUrl = p.ImageUrl,
                        Brand = p.Brand,
                        Model = p.Model,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching products with term '{searchTerm}' at 2025-07-31 08:48:25 UTC");
                return new List<ProductDTO>();
            }
        }

        public async Task<List<ProductDTO>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId)
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        ImageUrl = p.ImageUrl,
                        Brand = p.Brand,
                        Model = p.Model,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving products for category {categoryId} at 2025-07-31 08:48:25 UTC");
                return new List<ProductDTO>();
            }
        }

        public async Task<List<ProductDTO>> GetFeaturedProductsAsync(int count = 8)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        ImageUrl = p.ImageUrl,
                        Brand = p.Brand,
                        Model = p.Model,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving featured products at 2025-07-31 08:48:25 UTC");
                return new List<ProductDTO>();
            }
        }

        public async Task<List<ProductDTO>> GetRelatedProductsAsync(int productId, int count = 4)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return new List<ProductDTO>();

                return await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
                    .OrderBy(p => Guid.NewGuid())
                    .Take(count)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        ImageUrl = p.ImageUrl,
                        Brand = p.Brand,
                        Model = p.Model,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving related products for product {productId} at 2025-07-31 08:48:25 UTC");
                return new List<ProductDTO>();
            }
        }

        public async Task<List<CategoryDTO>> GetCategoriesAsync()
        {
            try
            {
                return await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving categories at 2025-07-31 08:48:25 UTC");
                return new List<CategoryDTO>();
            }
        }

        // Missing methods from interface
        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO createProductDto)
        {
            try
            {
                var product = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    CategoryId = createProductDto.CategoryId,
                    ImageUrl = createProductDto.ImageUrl,
                    Brand = createProductDto.Brand,
                    Model = createProductDto.Model,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product created by leshancha at 2025-07-31 08:48:25 UTC: {product.Name}");

                // ? FIXED: Handle potential null return more safely
                var createdProduct = await GetProductByIdAsync(product.Id);
                if (createdProduct == null)
                {
                    throw new InvalidOperationException($"Failed to retrieve created product with ID {product.Id}");
                }
                
                return createdProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating product at 2025-07-31 08:48:25 UTC");
                throw;
            }
        }

        public async Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO updateProductDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return null;

                if (!string.IsNullOrEmpty(updateProductDto.Name))
                    product.Name = updateProductDto.Name;
                if (!string.IsNullOrEmpty(updateProductDto.Description))
                    product.Description = updateProductDto.Description;
                if (updateProductDto.CategoryId.HasValue)
                    product.CategoryId = updateProductDto.CategoryId.Value;
                if (!string.IsNullOrEmpty(updateProductDto.ImageUrl))
                    product.ImageUrl = updateProductDto.ImageUrl;
                if (!string.IsNullOrEmpty(updateProductDto.Brand))
                    product.Brand = updateProductDto.Brand;
                if (!string.IsNullOrEmpty(updateProductDto.Model))
                    product.Model = updateProductDto.Model;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {id} updated by leshancha at 2025-07-31 08:48:25 UTC");

                return await GetProductByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id} at 2025-07-31 08:48:25 UTC");
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {id} deleted by leshancha at 2025-07-31 08:48:25 UTC");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id} at 2025-07-31 08:48:25 UTC");
                return false;
            }
        }

        public async Task<List<ProductInventoryDTO>> GetProductsWithInventoryAsync()
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.DistributorInventories)
                        .ThenInclude(di => di.Distributor)
                    .Select(p => new ProductInventoryDTO
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Brand = p.Brand,
                        Model = p.Model,
                        DistributorInventories = p.DistributorInventories
                            .Where(di => di.IsActive)
                            .Select(di => new DistributorInventoryDTO
                            {
                                DistributorId = di.DistributorId,
                                DistributorName = di.Distributor.CompanyName,
                                Price = di.Price,
                                Stock = di.Stock,
                                DeliveryDays = di.DeliveryDays,
                                IsActive = di.IsActive
                            }).ToList()
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving products with inventory at 2025-07-31 08:48:25 UTC");
                return new List<ProductInventoryDTO>();
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            try
            {
                return await _context.Products.AnyAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if product {id} exists at 2025-07-31 08:48:25 UTC");
                return false;
            }
        }
    }
}