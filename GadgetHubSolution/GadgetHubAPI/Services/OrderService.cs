using GadgetHubAPI.Data;
using GadgetHubAPI.DTOs;
using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<OrderDTO>> GetCustomerOrdersAsync(int customerId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Distributor)
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new OrderDTO
                    {
                        Id = o.Id,
                        CustomerId = o.CustomerId,
                        DistributorId = o.DistributorId,
                        DistributorName = o.Distributor.CompanyName,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        Notes = o.Notes,
                        EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                        ItemCount = o.Items.Count,
                        TotalItems = o.Items.Sum(oi => oi.Quantity)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving orders for customer {customerId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new List<OrderDTO>();
            }
        }

        public async Task<List<OrderDTO>> GetDistributorOrdersAsync(int distributorId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.DistributorId == distributorId)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new OrderDTO
                    {
                        Id = o.Id,
                        CustomerId = o.CustomerId,
                        CustomerName = o.Customer.Name,
                        DistributorId = o.DistributorId,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        Notes = o.Notes,
                        EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                        ItemCount = o.Items.Count,
                        TotalItems = o.Items.Sum(oi => oi.Quantity)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving orders for distributor {distributorId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new List<OrderDTO>();
            }
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Distributor)
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new OrderDTO
                    {
                        Id = o.Id,
                        CustomerId = o.CustomerId,
                        CustomerName = o.Customer.Name,
                        DistributorId = o.DistributorId,
                        DistributorName = o.Distributor.CompanyName,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        Notes = o.Notes,
                        EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                        ItemCount = o.Items.Count,
                        TotalItems = o.Items.Sum(oi => oi.Quantity)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving all orders at 2025-07-31 09:12:52 UTC by leshancha");
                return new List<OrderDTO>();
            }
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Distributor)
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.Id == orderId)
                    .Select(o => new OrderDTO
                    {
                        Id = o.Id,
                        CustomerId = o.CustomerId,
                        CustomerName = o.Customer.Name,
                        CustomerEmail = o.Customer.Email,
                        CustomerPhone = o.Customer.Phone,
                        CustomerAddress = o.Customer.Address,
                        DistributorId = o.DistributorId,
                        DistributorName = o.Distributor.CompanyName,
                        DistributorEmail = o.Distributor.Email,
                        DistributorPhone = o.Distributor.Phone,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        Notes = o.Notes,
                        EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                        ItemCount = o.Items.Count,
                        TotalItems = o.Items.Sum(oi => oi.Quantity)
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order {orderId} at 2025-07-31 09:12:52 UTC by leshancha");
                return null;
            }
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    CustomerId = createOrderDto.CustomerId,
                    DistributorId = createOrderDto.DistributorId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    Notes = createOrderDto.Notes,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(createOrderDto.EstimatedDeliveryDays ?? 7)
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemDto in createOrderDto.Items)
                {
                    var inventory = await _context.DistributorInventories
                        .FirstOrDefaultAsync(di => di.DistributorId == createOrderDto.DistributorId &&
                                                  di.ProductId == itemDto.ProductId && di.IsActive);

                    if (inventory == null || inventory.Stock < itemDto.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product {itemDto.ProductId}");
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = inventory.Price,
                        TotalPrice = inventory.Price * itemDto.Quantity
                    };

                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;

                    // Update inventory
                    inventory.Stock -= itemDto.Quantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                }

                order.TotalAmount = totalAmount;
                _context.OrderItems.AddRange(orderItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"Order {order.Id} created successfully for customer {createOrderDto.CustomerId} at 2025-07-31 09:12:52 UTC by leshancha");

                return await GetOrderByIdAsync(order.Id) ?? throw new InvalidOperationException("Failed to retrieve created order");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error creating order for customer {createOrderDto.CustomerId} at 2025-07-31 09:12:52 UTC by leshancha");
                throw;
            }
        }

        public async Task<OrderDTO?> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return null;

                order.Status = status;

                if (status.ToLower() == "delivered")
                {
                    order.EstimatedDeliveryDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {orderId} status updated to {status} at 2025-07-31 09:12:52 UTC by leshancha");

                return await GetOrderByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order {orderId} status at 2025-07-31 09:12:52 UTC by leshancha");
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId, string userType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return false;

                // Validate ownership
                if (!await ValidateOrderOwnershipAsync(orderId, userId, userType))
                {
                    throw new UnauthorizedAccessException("User does not have permission to cancel this order");
                }

                // Only allow cancellation for pending/confirmed orders
                if (order.Status.ToLower() == "delivered" || order.Status.ToLower() == "cancelled")
                {
                    throw new InvalidOperationException($"Cannot cancel order with status: {order.Status}");
                }

                // Restore inventory
                foreach (var item in order.Items)
                {
                    var inventory = await _context.DistributorInventories
                        .FirstOrDefaultAsync(di => di.DistributorId == order.DistributorId &&
                                                  di.ProductId == item.ProductId);

                    if (inventory != null)
                    {
                        inventory.Stock += item.Quantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                    }
                }

                order.Status = "Cancelled";
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"Order {orderId} cancelled by {userType} {userId} at 2025-07-31 09:12:52 UTC by leshancha");

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error cancelling order {orderId} at 2025-07-31 09:12:52 UTC by leshancha");
                throw;
            }
        }

        public async Task<List<OrderItemDTO>> GetOrderItemsAsync(int orderId)
        {
            try
            {
                return await _context.OrderItems
                    .Include(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                    .Where(oi => oi.OrderId == orderId)
                    .Select(oi => new OrderItemDTO
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductBrand = oi.Product.Brand,
                        ProductCategory = oi.Product.Category.Name,
                        ProductImage = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order items for order {orderId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new List<OrderItemDTO>();
            }
        }

        public async Task<OrderSummaryDTO> GetOrderSummaryAsync(int orderId)
        {
            try
            {
                var order = await GetOrderByIdAsync(orderId);
                if (order == null)
                    return new OrderSummaryDTO { OrderId = orderId, Found = false };

                var items = await GetOrderItemsAsync(orderId);

                return new OrderSummaryDTO
                {
                    OrderId = orderId,
                    Found = true,
                    Order = order,
                    Items = items,
                    Summary = new
                    {
                        TotalItems = items.Sum(i => i.Quantity),
                        TotalAmount = items.Sum(i => i.TotalPrice),
                        OrderDate = order.OrderDate,
                        Status = order.Status,
                        EstimatedDelivery = order.EstimatedDeliveryDate
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order summary for order {orderId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new OrderSummaryDTO { OrderId = orderId, Found = false };
            }
        }

        public async Task<bool> ValidateOrderOwnershipAsync(int orderId, int userId, string userType)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                return userType.ToLower() switch
                {
                    "customer" => order.CustomerId == userId,
                    "distributor" => order.DistributorId == userId,
                    "admin" => true,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating order ownership for order {orderId} at 2025-07-31 09:12:52 UTC by leshancha");
                return false;
            }
        }
    }
}