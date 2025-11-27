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
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationController> _logger;

        public QuotationController(IQuotationService quotationService, ILogger<QuotationController> logger)
        {
            _quotationService = quotationService;
            _logger = logger;
        }

        /// <summary>
        /// Get quotation requests for authenticated customer
        /// </summary>
        [HttpGet("customer/requests")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<List<QuotationRequestDTO>>> GetCustomerQuotationRequests()
        {
            try
            {
                var customerId = GetUserIdFromClaims("Customer");
                if (customerId == 0)
                {
                    return BadRequest(new { message = "Invalid customer ID", timestamp = DateTime.UtcNow });
                }

                var requests = await _quotationService.GetCustomerQuotationRequestsAsync(customerId);
                _logger.LogInformation($"Retrieved {requests.Count} quotation requests for customer {customerId} at 2025-07-31 09:18:28 UTC");

                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer quotation requests at 2025-07-31 09:18:28 UTC by leshancha");
                return StatusCode(500, new { message = "Error retrieving quotation requests", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Create new quotation request
        /// </summary>
        [HttpPost("request")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<object>> CreateQuotationRequest([FromBody] CreateQuotationRequestDTO createDto)
        {
            try
            {
                // ? FIXED: Better claim handling for both development and production
                var customerId = GetUserIdFromClaims("Customer");
                if (customerId == 0)
                {
                    return BadRequest(new { 
                        success = false,
                        message = "Invalid customer ID", 
                        timestamp = DateTime.UtcNow 
                    });
                }

                createDto.CustomerId = customerId;
                var request = await _quotationService.CreateQuotationRequestAsync(createDto);

                _logger.LogInformation($"? Quotation request {request.Id} created successfully by customer {customerId} by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                return Ok(new
                {
                    success = true,
                    message = "Quotation request submitted successfully! Distributors will respond within 24-48 hours.",
                    data = request,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error creating quotation request by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return StatusCode(500, new { 
                    success = false,
                    message = "Failed to create quotation request. Please try again.", 
                    error = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Get specific quotation request
        /// </summary>
        [HttpGet("request/{id}")]
        public async Task<ActionResult<QuotationRequestDTO>> GetQuotationRequest(int id)
        {
            try
            {
                var request = await _quotationService.GetQuotationRequestByIdAsync(id);
                if (request == null)
                {
                    return NotFound(new { message = $"Quotation request {id} not found", timestamp = DateTime.UtcNow });
                }

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation request {id} at 2025-07-31 09:18:28 UTC by leshancha");
                return StatusCode(500, new { message = "Error retrieving quotation request", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Get quotation comparison for a request
        /// </summary>
        [HttpGet("comparison/{requestId}")]
        public async Task<ActionResult<QuotationComparisonDTO>> GetQuotationComparison(int requestId)
        {
            try
            {
                var comparison = await _quotationService.GetQuotationComparisonAsync(requestId);
                if (!comparison.Found)
                {
                    return NotFound(new { message = $"Quotation request {requestId} not found", timestamp = DateTime.UtcNow });
                }

                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation comparison for request {requestId} at 2025-07-31 09:18:28 UTC by leshancha");
                return StatusCode(500, new { message = "Error retrieving quotation comparison", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Get quotation requests for authenticated distributor
        /// </summary>
        [HttpGet("distributor/requests")]
        [Authorize(Roles = "Distributor")]
        public async Task<ActionResult<List<QuotationRequestDTO>>> GetDistributorQuotationRequests([FromQuery] int? distributorId = null)
        {
            try
            {
                // Use provided distributorId or get from claims
                var currentDistributorId = distributorId ?? GetUserIdFromClaims("Distributor");
                if (currentDistributorId == 0)
                {
                    return BadRequest(new { message = "Invalid distributor ID", timestamp = DateTime.UtcNow });
                }

                var requests = await _quotationService.GetDistributorQuotationRequestsAsync(currentDistributorId);
                _logger.LogInformation($"Retrieved {requests.Count} quotation requests for distributor {currentDistributorId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by leshancha");

                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distributor quotation requests at {Timestamp} UTC by leshancha", DateTime.UtcNow);
                return StatusCode(500, new { message = "Error retrieving quotation requests", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Submit quotation response by distributor
        /// </summary>
        [HttpPost("response")]
        [Authorize(Roles = "Distributor")]
        public async Task<ActionResult<object>> SubmitQuotationResponse([FromBody] CreateQuotationResponseDTO createDto)
        {
            try
            {
                var distributorId = GetUserIdFromClaims("Distributor");
                if (distributorId == 0)
                {
                    return BadRequest(new { 
                        success = false,
                        message = "Invalid distributor ID", 
                        timestamp = DateTime.UtcNow 
                    });
                }

                // ? ENHANCED: Validate input data before processing
                if (createDto.Items == null || !createDto.Items.Any())
                {
                    return BadRequest(new { 
                        success = false,
                        message = "At least one item must be included in the quotation response",
                        timestamp = DateTime.UtcNow 
                    });
                }

                // ? ENHANCED: Log the ProductIds being submitted for debugging
                var submittedProductIds = createDto.Items.Select(i => i.ProductId).ToList();
                _logger.LogInformation($"?? Submitting quotation response with ProductIds: [{string.Join(", ", submittedProductIds)}]");

                createDto.DistributorId = distributorId;
                var response = await _quotationService.SubmitQuotationResponseAsync(createDto);

                _logger.LogInformation($"? Quotation response {response.Id} submitted successfully by distributor {distributorId} by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                return Ok(new
                {
                    success = true,
                    message = "Quotation response submitted successfully! Customer will be notified and can compare your offer.",
                    data = response,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"?? Invalid operation submitting quotation response by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return BadRequest(new { 
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error submitting quotation response by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return StatusCode(500, new { 
                    success = false,
                    message = "Failed to submit quotation response. Please try again.",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Get specific quotation response
        /// </summary>
        [HttpGet("response/{id}")]
        public async Task<ActionResult<QuotationResponseDTO>> GetQuotationResponse(int id)
        {
            try
            {
                var response = await _quotationService.GetQuotationResponseByIdAsync(id);
                if (response == null)
                {
                    return NotFound(new { message = $"Quotation response {id} not found", timestamp = DateTime.UtcNow });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation response {id} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by leshancha");
                return StatusCode(500, new { message = "Error retrieving quotation response", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Accept a quotation response and create an order
        /// </summary>
        [HttpPost("accept")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<OrderDTO>> AcceptQuotationResponse([FromBody] AcceptQuotationRequestDTO acceptDto)
        {
            try
            {
                var customerId = GetUserIdFromClaims("Customer");
                if (customerId == 0)
                {
                    return BadRequest(new { message = "Invalid customer ID", timestamp = DateTime.UtcNow });
                }

                var order = await _quotationService.AcceptQuotationAsync(acceptDto.ResponseId, customerId);

                _logger.LogInformation($"Quotation response {acceptDto.ResponseId} accepted by customer {customerId}, order {order.Id} created at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by leshancha");

                return Ok(order);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, $"Unauthorized access attempting to accept quotation at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Invalid operation accepting quotation at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error accepting quotation at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by leshancha");
                return StatusCode(500, new { message = "Error accepting quotation", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Cancel a quotation request
        /// </summary>
        [HttpPost("cancel/{requestId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> CancelQuotationRequest(int requestId)
        {
            try
            {
                var customerId = GetUserIdFromClaims("Customer");
                if (customerId == 0)
                {
                    return BadRequest(new { message = "Invalid customer ID", timestamp = DateTime.UtcNow });
                }

                var result = await _quotationService.CancelQuotationRequestAsync(requestId, customerId);
                if (!result)
                {
                    return NotFound(new { message = "Quotation request not found", timestamp = DateTime.UtcNow });
                }

                _logger.LogInformation($"Quotation request {requestId} cancelled by customer {customerId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by leshancha");

                return Ok(new { message = "Quotation request cancelled successfully", timestamp = DateTime.UtcNow });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Invalid operation cancelling quotation at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return BadRequest(new { message = ex.Message, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling quotation request {requestId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC by leshancha");
                return StatusCode(500, new { message = "Error cancelling quotation request", timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Get quotation request items for a specific request
        /// </summary>
        [HttpGet("request/{requestId}/items")]
        public async Task<ActionResult<List<QuotationRequestItemDTO>>> GetQuotationRequestItems(int requestId)
        {
            try
            {
                var items = await _quotationService.GetQuotationRequestItemsAsync(requestId);
                
                if (!items.Any())
                {
                    _logger.LogWarning($"?? No items found for quotation request {requestId}");
                    return NotFound(new { 
                        message = $"No items found for quotation request {requestId}", 
                        timestamp = DateTime.UtcNow 
                    });
                }

                _logger.LogInformation($"?? Retrieved {items.Count} items for quotation request {requestId}");
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error retrieving items for quotation request {requestId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return StatusCode(500, new { 
                    message = "Error retrieving quotation request items", 
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Get distributor's own quotation response for a specific request
        /// </summary>
        [HttpGet("distributor/{requestId}/response")]
        [Authorize(Roles = "Distributor")]
        public async Task<ActionResult<QuotationResponseDTO>> GetDistributorQuotationResponse(int requestId, [FromQuery] int? distributorId = null)
        {
            try
            {
                // Use provided distributorId or get from claims
                var currentDistributorId = distributorId ?? GetUserIdFromClaims("Distributor");
                if (currentDistributorId == 0)
                {
                    return BadRequest(new { message = "Invalid distributor ID", timestamp = DateTime.UtcNow });
                }

                var response = await _quotationService.GetDistributorQuotationResponseAsync(requestId, currentDistributorId);
                
                if (response == null)
                {
                    _logger.LogWarning($"?? No quotation response found for request {requestId}, distributor {currentDistributorId}");
                    return NotFound(new { 
                        message = $"No quotation response found for request {requestId}", 
                        timestamp = DateTime.UtcNow 
                    });
                }

                _logger.LogInformation($"?? Retrieved quotation response for request {requestId}, distributor {currentDistributorId}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error retrieving distributor quotation response for request {requestId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return StatusCode(500, new { 
                    message = "Error retrieving quotation response", 
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Update an existing quotation response
        /// </summary>
        [HttpPut("response/{responseId}")]
        [Authorize(Roles = "Distributor")]
        public async Task<ActionResult<QuotationResponseDTO>> UpdateQuotationResponse(int responseId, [FromBody] CreateQuotationResponseDTO updateDto)
        {
            try
            {
                var distributorId = GetUserIdFromClaims("Distributor");
                if (distributorId == 0)
                {
                    return BadRequest(new { message = "Invalid distributor ID", timestamp = DateTime.UtcNow });
                }

                // Verify the response belongs to this distributor
                var existingResponse = await _quotationService.GetQuotationResponseByIdAsync(responseId);
                if (existingResponse == null)
                {
                    return NotFound(new { message = "Quotation response not found", timestamp = DateTime.UtcNow });
                }

                if (existingResponse.DistributorId != distributorId)
                {
                    return Forbid();
                }

                updateDto.DistributorId = distributorId;
                var updatedResponse = await _quotationService.UpdateQuotationResponseAsync(responseId, updateDto);

                _logger.LogInformation($"? Quotation response {responseId} updated by distributor {distributorId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                return Ok(new
                {
                    success = true,
                    message = "Quotation response updated successfully",
                    data = updatedResponse,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"?? Invalid operation updating quotation response {responseId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return BadRequest(new { 
                    success = false,
                    message = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error updating quotation response {responseId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return StatusCode(500, new { 
                    success = false,
                    message = "Error updating quotation response", 
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Check if distributor has already responded to a quotation request
        /// </summary>
        [HttpGet("request/{requestId}/has-response")]
        [Authorize(Roles = "Distributor")]
        public async Task<ActionResult<object>> HasDistributorResponded(int requestId, [FromQuery] int? distributorId = null)
        {
            try
            {
                // Use provided distributorId or get from claims
                var currentDistributorId = distributorId ?? GetUserIdFromClaims("Distributor");
                if (currentDistributorId == 0)
                {
                    return BadRequest(new { message = "Invalid distributor ID", timestamp = DateTime.UtcNow });
                }

                var hasResponded = await _quotationService.HasDistributorRespondedAsync(requestId, currentDistributorId);

                return Ok(new
                {
                    requestId = requestId,
                    distributorId = currentDistributorId,
                    hasResponded = hasResponded,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error checking response status for request {requestId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return StatusCode(500, new { 
                    message = "Error checking response status", 
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Helper method to get user ID from claims with fallback for development mode
        /// </summary>
        private int GetUserIdFromClaims(string userType)
        {
            try
            {
                // Try specific claim type first (e.g., "CustomerId", "DistributorId")
                var specificClaim = User.FindFirst($"{userType}Id")?.Value;
                if (!string.IsNullOrEmpty(specificClaim) && int.TryParse(specificClaim, out var specificId))
                {
                    return specificId;
                }

                // Try standard NameIdentifier claim
                var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(nameIdentifierClaim) && int.TryParse(nameIdentifierClaim, out var nameId))
                {
                    return nameId;
                }

                // Try UserId claim (fallback)
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                // Try "sub" claim (JWT standard)
                var subClaim = User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(subClaim) && int.TryParse(subClaim, out var subId))
                {
                    return subId;
                }

                // Development mode fallback
                _logger.LogWarning($"?? No valid user ID found in claims for {userType}. Using development fallback.");
                return 1; // Default development user ID
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error extracting user ID from claims for {userType}");
                return 1; // Development fallback
            }
        }
    }
}