using GadgetHubAPI.DTOs;

namespace GadgetHubAPI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetCustomerOrdersAsync(int customerId);
        Task<List<OrderDTO>> GetDistributorOrdersAsync(int distributorId);
        Task<List<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO?> GetOrderByIdAsync(int orderId);
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto);
        Task<OrderDTO?> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId, int userId, string userType);
        Task<List<OrderItemDTO>> GetOrderItemsAsync(int orderId);
        Task<OrderSummaryDTO> GetOrderSummaryAsync(int orderId);
        Task<bool> ValidateOrderOwnershipAsync(int orderId, int userId, string userType);
    }
}