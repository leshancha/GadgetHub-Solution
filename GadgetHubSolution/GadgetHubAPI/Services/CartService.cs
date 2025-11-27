using GadgetHubAPI.Data;
using GadgetHubAPI.DTOs;
using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CartSummaryDTO> GetCustomerCartAsync(int customerId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CustomerId == customerId)
                    .OrderByDescending(ci => ci.DateAdded)
                    .ToListAsync();

                var cartItemDtos = cartItems.Select(ci => new CartItemDTO
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl,
                    Brand = ci.Product.Brand,
                    Quantity = ci.Quantity,
                    DateAdded = ci.DateAdded
                }).ToList();

                return new CartSummaryDTO
                {
                    Items = cartItemDtos,
                    TotalItems = cartItems.Sum(ci => ci.Quantity),
                    LastUpdated = cartItems.Any() ? cartItems.Max(ci => ci.DateAdded) : DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cart for customer {customerId} at 2025-07-31 08:44:57 UTC");
                return new CartSummaryDTO();
            }
        }

        public async Task<CartSummaryDTO> GetGuestCartAsync(string sessionId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.SessionId == sessionId && ci.CustomerId == null)
                    .OrderByDescending(ci => ci.DateAdded)
                    .ToListAsync();

                var cartItemDtos = cartItems.Select(ci => new CartItemDTO
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl,
                    Brand = ci.Product.Brand,
                    Quantity = ci.Quantity,
                    DateAdded = ci.DateAdded
                }).ToList();

                return new CartSummaryDTO
                {
                    Items = cartItemDtos,
                    TotalItems = cartItems.Sum(ci => ci.Quantity),
                    LastUpdated = cartItems.Any() ? cartItems.Max(ci => ci.DateAdded) : DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving guest cart {sessionId} at 2025-07-31 08:44:57 UTC");
                return new CartSummaryDTO();
            }
        }

        public async Task AddToCustomerCartAsync(int customerId, int productId, int quantity)
        {
            try
            {
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.DateAdded = DateTime.UtcNow;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CustomerId = customerId,
                        ProductId = productId,
                        Quantity = quantity,
                        DateAdded = DateTime.UtcNow
                    };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added product {productId} to customer {customerId} cart at 2025-07-31 08:44:57 UTC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product {productId} to customer {customerId} cart");
                throw;
            }
        }

        public async Task AddToGuestCartAsync(string sessionId, int productId, int quantity)
        {
            try
            {
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.SessionId == sessionId && ci.ProductId == productId && ci.CustomerId == null);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.DateAdded = DateTime.UtcNow;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        SessionId = sessionId,
                        ProductId = productId,
                        Quantity = quantity,
                        DateAdded = DateTime.UtcNow
                    };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added product {productId} to guest cart {sessionId} at 2025-07-31 08:44:57 UTC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product {productId} to guest cart {sessionId}");
                throw;
            }
        }

        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
            try
            {
                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem != null)
                {
                    if (quantity <= 0)
                    {
                        _context.CartItems.Remove(cartItem);
                    }
                    else
                    {
                        cartItem.Quantity = quantity;
                        cartItem.DateAdded = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated cart item {cartItemId} quantity to {quantity} at 2025-07-31 08:44:57 UTC");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart item {cartItemId}");
                throw;
            }
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Removed cart item {cartItemId} at 2025-07-31 08:44:57 UTC");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cart item {cartItemId}");
                throw;
            }
        }

        public async Task ClearCustomerCartAsync(int customerId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CustomerId == customerId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cleared cart for customer {customerId} at 2025-07-31 08:44:57 UTC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing cart for customer {customerId}");
                throw;
            }
        }

        public async Task ClearGuestCartAsync(string sessionId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.SessionId == sessionId && ci.CustomerId == null)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cleared guest cart {sessionId} at 2025-07-31 08:44:57 UTC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing guest cart {sessionId}");
                throw;
            }
        }

        public async Task MergeGuestCartAsync(int customerId, string sessionId)
        {
            try
            {
                var guestCartItems = await _context.CartItems
                    .Where(ci => ci.SessionId == sessionId && ci.CustomerId == null)
                    .ToListAsync();

                foreach (var guestItem in guestCartItems)
                {
                    var existingCustomerItem = await _context.CartItems
                        .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.ProductId == guestItem.ProductId);

                    if (existingCustomerItem != null)
                    {
                        existingCustomerItem.Quantity += guestItem.Quantity;
                        existingCustomerItem.DateAdded = DateTime.UtcNow;
                    }
                    else
                    {
                        guestItem.CustomerId = customerId;
                        guestItem.SessionId = null;
                        guestItem.DateAdded = DateTime.UtcNow;
                    }
                }

                // Remove duplicate guest items that were merged
                var duplicateGuestItems = guestCartItems.Where(gi =>
                    _context.CartItems.Any(ci => ci.CustomerId == customerId && ci.ProductId == gi.ProductId && ci.Id != gi.Id));
                _context.CartItems.RemoveRange(duplicateGuestItems);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Merged guest cart {sessionId} with customer {customerId} at 2025-07-31 08:44:57 UTC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error merging guest cart {sessionId} with customer {customerId}");
                throw;
            }
        }

        public async Task<int> GetCartCountAsync(int? customerId = null, string? sessionId = null)
        {
            try
            {
                if (customerId.HasValue)
                {
                    return await _context.CartItems
                        .Where(ci => ci.CustomerId == customerId.Value)
                        .SumAsync(ci => ci.Quantity);
                }
                else if (!string.IsNullOrEmpty(sessionId))
                {
                    return await _context.CartItems
                        .Where(ci => ci.SessionId == sessionId && ci.CustomerId == null)
                        .SumAsync(ci => ci.Quantity);
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return 0;
            }
        }
    }
}