using GadgetHubAPI.DTOs;
using GadgetHubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GadgetHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Get orders for authenticated customer
        /// </summary>
        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<List<OrderDTO>>> GetCustomerOrders()
        {
            try
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (!int.TryParse(customerIdClaim, out var customerId))
                {
                    return BadRequest(new { message = "Invalid customer ID", timestamp = DateTime.UtcNow });
                }

                var orders = await _orderService.GetCustomerOrdersAsync(customerId);
                _logger.LogInformation($"Retrieved {orders.Count} orders for customer {customerId} at 2025-07-31 09:18:28 UTC");

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer orders at 2025-07-31 09:18:28 UTC by leshancha");
                return StatusCode(500, new { message = "Error retrieving orders", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Get orders for authenticated distributor
        /// </summary>
        [HttpGet("distributor")]
        [Authorize(Roles = "Distributor")]
        public async Task<ActionResult<List<OrderDTO>>> GetDistributorOrders([FromQuery] int? distributorId = null)
        {
            try
            {
                // Use provided distributorId or get from claims
                var currentDistributorId = distributorId;
                if (!currentDistributorId.HasValue)
                {
                    var distributorIdClaim = User.FindFirst("DistributorId")?.Value ?? User.FindFirst("UserId")?.Value;
                    if (!int.TryParse(distributorIdClaim, out var claimDistributorId))
                    {
                        return BadRequest(new { message = "Invalid distributor ID", timestamp = DateTime.UtcNow });
                    }
                    currentDistributorId = claimDistributorId;
                }

                var orders = await _orderService.GetDistributorOrdersAsync(currentDistributorId.Value);
                _logger.LogInformation($"Retrieved {orders.Count} orders for distributor {currentDistributorId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by leshancha");

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distributor orders at {Timestamp} UTC by leshancha", DateTime.UtcNow);
                return StatusCode(500, new { message = "Error retrieving orders", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Get specific order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Order {id} not found", timestamp = DateTime.UtcNow });
                }

                // Validate ownership
                var userType = User.FindFirst("Role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(User.FindFirst($"{userType}Id")?.Value ?? "0");

                if (!await _orderService.ValidateOrderOwnershipAsync(id, userId, userType ?? ""))
                {
                    return Forbid();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order {id} at 2025-07-31 09:18:28 UTC by leshancha");
                return StatusCode(500, new { message = "Error retrieving order", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Create new order
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<OrderDTO>> CreateOrder([FromBody] CreateOrderDTO createOrderDto)
        {
            try
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (!int.TryParse(customerIdClaim, out var customerId))
                {
                    return BadRequest(new { message = "Invalid customer ID", timestamp = DateTime.UtcNow });
                }

                createOrderDto.CustomerId = customerId;
                var order = await _orderService.CreateOrderAsync(createOrderDto);

                _logger.LogInformation($"Order {order.Id} created by customer {customerId} at 2025-07-31 09:18:28 UTC by leshancha");

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order at 2025-07-31 09:18:28 UTC by leshancha");
                return StatusCode(500, new { message = "Error creating order", timestamp = DateTime.UtcNow });
            }
        }
    }
}