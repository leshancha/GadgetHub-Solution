using GadgetHubAPI.Data;
using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Services
{
    public class DistributorService : IDistributorService
    {
        private readonly ApplicationDbContext _context;

        public DistributorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DistributorInventory>> GetInventoryAsync(int distributorId)
        {
            return await _context.DistributorInventories
                .Include(di => di.Product)
                    .ThenInclude(p => p.Category)
                .Include(di => di.Distributor)
                .Where(di => di.DistributorId == distributorId && di.IsActive)
                .OrderBy(di => di.Product.Name)
                .ToListAsync();
        }

        public async Task<DistributorInventory?> GetInventoryItemAsync(int distributorId, int productId)
        {
            return await _context.DistributorInventories
                .Include(di => di.Product)
                    .ThenInclude(p => p.Category)
                .Include(di => di.Distributor)
                .FirstOrDefaultAsync(di => di.DistributorId == distributorId && di.ProductId == productId && di.IsActive);
        }

        public async Task<DistributorInventory> UpdateInventoryAsync(int distributorId, int productId, decimal price, int stock, int deliveryDays)
        {
            var existingInventory = await _context.DistributorInventories
                .FirstOrDefaultAsync(di => di.DistributorId == distributorId && di.ProductId == productId);

            if (existingInventory != null)
            {
                existingInventory.Price = price;
                existingInventory.Stock = stock;
                existingInventory.DeliveryDays = deliveryDays;
                existingInventory.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                existingInventory = new DistributorInventory
                {
                    DistributorId = distributorId,
                    ProductId = productId,
                    Price = price,
                    Stock = stock,
                    DeliveryDays = deliveryDays
                };
                _context.DistributorInventories.Add(existingInventory);
            }

            await _context.SaveChangesAsync();

            return await GetInventoryItemAsync(distributorId, productId) ?? existingInventory;
        }

        public async Task<bool> DeleteInventoryItemAsync(int distributorId, int productId)
        {
            var inventory = await _context.DistributorInventories
                .FirstOrDefaultAsync(di => di.DistributorId == distributorId && di.ProductId == productId);

            if (inventory == null)
                return false;

            inventory.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}