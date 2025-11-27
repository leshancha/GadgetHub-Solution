using GadgetHubAPI.DTOs;

namespace GadgetHubAPI.Services
{
    public interface ICartService
    {
        Task<CartSummaryDTO> GetCustomerCartAsync(int customerId);
        Task<CartSummaryDTO> GetGuestCartAsync(string sessionId);
        Task AddToCustomerCartAsync(int customerId, int productId, int quantity);
        Task AddToGuestCartAsync(string sessionId, int productId, int quantity);
        Task UpdateCartItemAsync(int cartItemId, int quantity);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCustomerCartAsync(int customerId);
        Task ClearGuestCartAsync(string sessionId);
        Task MergeGuestCartAsync(int customerId, string sessionId);
        Task<int> GetCartCountAsync(int? customerId = null, string? sessionId = null);
    }
}