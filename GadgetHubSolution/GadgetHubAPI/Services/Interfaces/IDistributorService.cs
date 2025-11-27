using GadgetHubAPI.Models;

namespace GadgetHubAPI.Services
{
    public interface IDistributorService
    {
        Task<List<DistributorInventory>> GetInventoryAsync(int distributorId);
        Task<DistributorInventory?> GetInventoryItemAsync(int distributorId, int productId);
        Task<DistributorInventory> UpdateInventoryAsync(int distributorId, int productId, decimal price, int stock, int deliveryDays);
        Task<bool> DeleteInventoryItemAsync(int distributorId, int productId);
    }
}