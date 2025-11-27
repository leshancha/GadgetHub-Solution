using GadgetHubAPI.DTOs;

namespace GadgetHubAPI.Services
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(int id);
        Task<List<ProductDTO>> SearchProductsAsync(string searchTerm);
        Task<List<ProductDTO>> GetProductsByCategoryAsync(int categoryId);
        Task<List<ProductDTO>> GetFeaturedProductsAsync(int count = 8);
        Task<List<ProductDTO>> GetRelatedProductsAsync(int productId, int count = 4);
        Task<List<CategoryDTO>> GetCategoriesAsync();
        Task<ProductDTO> CreateProductAsync(CreateProductDTO createProductDto);
        Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO updateProductDto);
        Task<bool> DeleteProductAsync(int id);
        Task<List<ProductInventoryDTO>> GetProductsWithInventoryAsync();
        Task<bool> ProductExistsAsync(int id);
    }
}