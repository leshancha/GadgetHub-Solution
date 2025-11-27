using GadgetHubAPI.Data;
using GadgetHubAPI.DTOs;
using GadgetHubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetHubAPI.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly ILogger<QuotationService> _logger;

        public QuotationService(ApplicationDbContext context, IOrderService orderService, ILogger<QuotationService> logger)
        {
            _context = context;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<List<QuotationRequestDTO>> GetCustomerQuotationRequestsAsync(int customerId)
        {
            try
            {
                return await _context.QuotationRequests
                    .Include(qr => qr.Items)
                        .ThenInclude(qri => qri.Product)
                    .Include(qr => qr.Responses)
                    .Where(qr => qr.CustomerId == customerId)
                    .OrderByDescending(qr => qr.RequestDate)
                    .Select(qr => new QuotationRequestDTO
                    {
                        Id = qr.Id,
                        CustomerId = qr.CustomerId,
                        RequestDate = qr.RequestDate,
                        Status = qr.Status,
                        Notes = qr.Notes,
                        ItemCount = qr.Items.Count,
                        TotalItems = qr.Items.Sum(qri => qri.Quantity),
                        ResponseCount = qr.Responses.Count,
                        HasResponses = qr.Responses.Any()
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation requests for customer {customerId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new List<QuotationRequestDTO>();
            }
        }

        public async Task<List<QuotationRequestDTO>> GetDistributorQuotationRequestsAsync(int distributorId)
        {
            try
            {
                // ? FIXED: Get ALL pending requests that distributors can respond to
                // Show all pending requests, not just those where distributor has products in stock
                return await _context.QuotationRequests
                    .Include(qr => qr.Customer)
                    .Include(qr => qr.Items)
                        .ThenInclude(qri => qri.Product)
                    .Include(qr => qr.Responses)
                    .Where(qr => qr.Status == "Pending")
                    .OrderByDescending(qr => qr.RequestDate)
                    .Select(qr => new QuotationRequestDTO
                    {
                        Id = qr.Id,
                        CustomerId = qr.CustomerId,
                        CustomerName = qr.Customer.Name,
                        CustomerEmail = qr.Customer.Email,
                        RequestDate = qr.RequestDate,
                        Status = qr.Status,
                        Notes = qr.Notes,
                        ItemCount = qr.Items.Count,
                        TotalItems = qr.Items.Sum(qri => qri.Quantity),
                        ResponseCount = qr.Responses.Count,
                        HasResponses = qr.Responses.Any(),
                        AlreadyResponded = qr.Responses.Any(qresp => qresp.DistributorId == distributorId),
                        // ? ENHANCED: Add indicator if distributor has any of the requested products
                        CanRespond = qr.Items.Any(qri => _context.DistributorInventories
                            .Any(di => di.DistributorId == distributorId &&
                                      di.ProductId == qri.ProductId &&
                                      di.IsActive)) || true // Always allow response - distributor can add products
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation requests for distributor {distributorId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new List<QuotationRequestDTO>();
            }
        }

        public async Task<QuotationRequestDTO?> GetQuotationRequestByIdAsync(int requestId)
        {
            try
            {
                return await _context.QuotationRequests
                    .Include(qr => qr.Customer)
                    .Include(qr => qr.Items)
                        .ThenInclude(qri => qri.Product)
                            .ThenInclude(p => p.Category)
                    .Include(qr => qr.Responses)
                        .ThenInclude(qresp => qresp.Distributor)
                    .Where(qr => qr.Id == requestId)
                    .Select(qr => new QuotationRequestDTO
                    {
                        Id = qr.Id,
                        CustomerId = qr.CustomerId,
                        CustomerName = qr.Customer.Name,
                        CustomerEmail = qr.Customer.Email,
                        CustomerPhone = qr.Customer.Phone,
                        CustomerAddress = qr.Customer.Address,
                        RequestDate = qr.RequestDate,
                        Status = qr.Status,
                        Notes = qr.Notes,
                        RequiredByDate = qr.RequiredByDate,
                        DeliveryAddress = qr.DeliveryAddress,
                        ContactPhone = qr.ContactPhone,
                        ItemCount = qr.Items.Count,
                        TotalItems = qr.Items.Sum(qri => qri.Quantity),
                        ResponseCount = qr.Responses.Count,
                        HasResponses = qr.Responses.Any(),
                        // ? ENHANCED: Add items to DTO
                        Items = qr.Items.Select(qri => new QuotationRequestItemDTO
                        {
                            Id = qri.Id,
                            QuotationRequestId = qri.QuotationRequestId,
                            ProductId = qri.ProductId,
                            ProductName = qri.Product.Name,
                            ProductBrand = qri.Product.Brand,
                            ProductCategory = qri.Product.Category.Name,
                            ProductImage = qri.Product.ImageUrl,
                            Quantity = qri.Quantity,
                            Specifications = qri.Specifications
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error retrieving quotation request {requestId} by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return null;
            }
        }

        public async Task<QuotationRequestDTO> CreateQuotationRequestAsync(CreateQuotationRequestDTO createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ? ENHANCED: Validate required data
                if (createDto.CustomerId <= 0)
                {
                    throw new InvalidOperationException("Invalid customer ID provided");
                }

                // ? NEW: Validate customer exists in database
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == createDto.CustomerId);
                if (!customerExists)
                {
                    _logger.LogError($"? Customer ID {createDto.CustomerId} not found in database");
                    throw new InvalidOperationException($"Customer with ID {createDto.CustomerId} does not exist");
                }

                if (createDto.Items == null || !createDto.Items.Any())
                {
                    throw new InvalidOperationException("Quotation request must have at least one item");
                }

                var quotationRequest = new QuotationRequest
                {
                    CustomerId = createDto.CustomerId,
                    RequestDate = DateTime.UtcNow,
                    Status = "Pending",
                    Notes = createDto.Notes ?? "",
                    RequiredByDate = createDto.RequiredByDate == default ? DateTime.UtcNow.AddDays(7) : createDto.RequiredByDate,
                    DeliveryAddress = createDto.DeliveryAddress ?? "",
                    ContactPhone = createDto.ContactPhone ?? ""
                };

                _context.QuotationRequests.Add(quotationRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"? QuotationRequest saved to database with ID: {quotationRequest.Id}");

                var requestItems = new List<QuotationRequestItem>();
                foreach (var itemDto in createDto.Items)
                {
                    // ? ENHANCED: Validate product exists
                    var productExists = await _context.Products.AnyAsync(p => p.Id == itemDto.ProductId);
                    if (!productExists)
                    {
                        _logger.LogWarning($"?? Product ID {itemDto.ProductId} not found, skipping item");
                        continue;
                    }

                    var requestItem = new QuotationRequestItem
                    {
                        QuotationRequestId = quotationRequest.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        Specifications = itemDto.Specifications ?? ""
                    };
                    requestItems.Add(requestItem);
                }

                if (!requestItems.Any())
                {
                    throw new InvalidOperationException("No valid products found for quotation request");
                }

                _context.QuotationRequestItems.AddRange(requestItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"? Added {requestItems.Count} items to QuotationRequest {quotationRequest.Id}");

                await transaction.CommitAsync();

                _logger.LogInformation($"? Quotation request {quotationRequest.Id} created successfully for customer {createDto.CustomerId} with {requestItems.Count} items by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                // ? ENHANCED: Return detailed DTO
                var result = await GetQuotationRequestByIdAsync(quotationRequest.Id);
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to retrieve created quotation request");
                }

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"? Error creating quotation request for customer {createDto.CustomerId} by leshancha at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                throw;
            }
        }

        public async Task<QuotationResponseDTO> SubmitQuotationResponseAsync(CreateQuotationResponseDTO responseDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if distributor already responded
                var existingResponse = await _context.QuotationResponses
                    .FirstOrDefaultAsync(qr => qr.QuotationRequestId == responseDto.QuotationRequestId &&
                                              qr.DistributorId == responseDto.DistributorId);

                if (existingResponse != null)
                {
                    throw new InvalidOperationException("Distributor has already responded to this quotation request");
                }

                // ? ADDED: Validate that all ProductIds exist in the database
                var requestedProductIds = responseDto.Items.Select(i => i.ProductId).Distinct().ToList();
                var existingProducts = await _context.Products
                    .Where(p => requestedProductIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var missingProductIds = requestedProductIds.Except(existingProducts).ToList();
                if (missingProductIds.Any())
                {
                    var errorMessage = $"The following ProductIds do not exist in the database: {string.Join(", ", missingProductIds)}";
                    _logger.LogError($"? Foreign key constraint would fail: {errorMessage}");
                    throw new InvalidOperationException(errorMessage);
                }

                var quotationResponse = new QuotationResponse
                {
                    QuotationRequestId = responseDto.QuotationRequestId,
                    DistributorId = responseDto.DistributorId,
                    SubmissionDate = DateTime.UtcNow,
                    Status = "Submitted",
                    Notes = responseDto.Notes
                };

                _context.QuotationResponses.Add(quotationResponse);
                await _context.SaveChangesAsync();

                decimal totalPrice = 0;
                var responseItems = new List<QuotationResponseItem>();

                foreach (var itemDto in responseDto.Items)
                {
                    // ? ENHANCED: Additional validation for each item
                    if (!existingProducts.Contains(itemDto.ProductId))
                    {
                        _logger.LogWarning($"?? Skipping invalid ProductId {itemDto.ProductId} in quotation response");
                        continue;
                    }

                    var responseItem = new QuotationResponseItem
                    {
                        QuotationResponseId = quotationResponse.Id,
                        ProductId = itemDto.ProductId,
                        UnitPrice = itemDto.UnitPrice,
                        Stock = itemDto.Stock,
                        DeliveryDays = itemDto.DeliveryDays,
                        Quantity = itemDto.Quantity,
                        TotalPrice = itemDto.UnitPrice * itemDto.Quantity
                    };

                    responseItems.Add(responseItem);
                    totalPrice += responseItem.TotalPrice;
                }

                if (!responseItems.Any())
                {
                    throw new InvalidOperationException("No valid response items were created. All ProductIds were invalid.");
                }

                quotationResponse.TotalPrice = totalPrice;
                _context.QuotationResponseItems.AddRange(responseItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"? Quotation response submitted by distributor {responseDto.DistributorId} for request {responseDto.QuotationRequestId} with {responseItems.Count} valid items at 2025-07-31 09:12:52 UTC by leshancha");

                return await GetQuotationResponseByIdAsync(quotationResponse.Id) ??
                    throw new InvalidOperationException("Failed to retrieve created quotation response");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error submitting quotation response at 2025-07-31 09:12:52 UTC by leshancha");
                throw;
            }
        }

        public async Task<List<QuotationResponseDTO>> GetQuotationResponsesAsync(int requestId)
        {
            try
            {
                return await _context.QuotationResponses
                    .Include(qr => qr.Distributor)
                    .Include(qr => qr.Items)
                        .ThenInclude(qri => qri.Product)
                    .Where(qr => qr.QuotationRequestId == requestId)
                    .OrderBy(qr => qr.TotalPrice)
                    .Select(qr => new QuotationResponseDTO
                    {
                        Id = qr.Id,
                        QuotationRequestId = qr.QuotationRequestId,
                        DistributorId = qr.DistributorId,
                        DistributorName = qr.Distributor.CompanyName,
                        DistributorEmail = qr.Distributor.Email,
                        DistributorPhone = qr.Distributor.Phone,
                        TotalPrice = qr.TotalPrice,
                        SubmissionDate = qr.SubmissionDate,
                        Status = qr.Status,
                        Notes = qr.Notes,
                        ItemCount = qr.Items.Count,
                        AverageDeliveryDays = (int)qr.Items.Average(qri => qri.DeliveryDays)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation responses for request {requestId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new List<QuotationResponseDTO>();
            }
        }

        public async Task<QuotationComparisonDTO> GetQuotationComparisonAsync(int requestId)
        {
            try
            {
                var request = await GetQuotationRequestByIdAsync(requestId);
                if (request == null)
                    return new QuotationComparisonDTO { RequestId = requestId, Found = false };

                var responses = await GetQuotationResponsesAsync(requestId);
                var requestItems = await GetQuotationRequestItemsAsync(requestId);

                return new QuotationComparisonDTO
                {
                    RequestId = requestId,
                    Found = true,
                    QuotationRequest = request,
                    Responses = responses,
                    RequestItems = requestItems,
                    BestPrice = responses.Any() ? responses.Min(r => r.TotalPrice) : 0,
                    WorstPrice = responses.Any() ? responses.Max(r => r.TotalPrice) : 0,
                    AveragePrice = responses.Any() ? responses.Average(r => r.TotalPrice) : 0,
                    BestDelivery = responses.Any() ? responses.Min(r => r.AverageDeliveryDays) : 0,
                    ResponseCount = responses.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting quotation comparison for request {requestId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new QuotationComparisonDTO { RequestId = requestId, Found = false };
            }
        }

        public async Task<OrderDTO> AcceptQuotationAsync(int responseId, int customerId)
        {
            try
            {
                var response = await _context.QuotationResponses
                    .Include(qr => qr.QuotationRequest)
                    .Include(qr => qr.Items)
                    .FirstOrDefaultAsync(qr => qr.Id == responseId);

                if (response == null)
                    throw new InvalidOperationException("Quotation response not found");

                if (response.QuotationRequest.CustomerId != customerId)
                    throw new UnauthorizedAccessException("Customer does not own this quotation request");

                if (response.Status != "Submitted")
                    throw new InvalidOperationException($"Cannot accept quotation with status: {response.Status}");

                // Create order from quotation response
                var createOrderDto = new CreateOrderDTO
                {
                    CustomerId = customerId,
                    DistributorId = response.DistributorId,
                    Notes = $"Order created from quotation response {responseId}",
                    EstimatedDeliveryDays = (int)response.Items.Average(qri => qri.DeliveryDays),
                    Items = response.Items.Select(qri => new CreateOrderItemDTO
                    {
                        ProductId = qri.ProductId,
                        Quantity = qri.Quantity
                    }).ToList()
                };

                var order = await _orderService.CreateOrderAsync(createOrderDto);

                // Update quotation statuses
                response.Status = "Accepted";
                response.QuotationRequest.Status = "Completed";

                // Mark other responses as rejected
                var otherResponses = await _context.QuotationResponses
                    .Where(qr => qr.QuotationRequestId == response.QuotationRequestId && qr.Id != responseId)
                    .ToListAsync();

                foreach (var otherResponse in otherResponses)
                {
                    otherResponse.Status = "Rejected";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Quotation response {responseId} accepted by customer {customerId}, order {order.Id} created at 2025-07-31 09:12:52 UTC by leshancha");

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error accepting quotation {responseId} at 2025-07-31 09:12:52 UTC by leshancha");
                throw;
            }
        }

        public async Task<bool> CancelQuotationRequestAsync(int requestId, int customerId)
        {
            try
            {
                var request = await _context.QuotationRequests
                    .Include(qr => qr.Responses)
                    .FirstOrDefaultAsync(qr => qr.Id == requestId && qr.CustomerId == customerId);

                if (request == null) return false;

                if (request.Status == "Completed" || request.Status == "Cancelled")
                    throw new InvalidOperationException($"Cannot cancel quotation with status: {request.Status}");

                request.Status = "Cancelled";

                // Mark all responses as cancelled
                foreach (var response in request.Responses)
                {
                    response.Status = "Cancelled";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Quotation request {requestId} cancelled by customer {customerId} at 2025-07-31 09:12:52 UTC by leshancha");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling quotation request {requestId} at 2025-07-31 09:12:52 UTC by leshancha");
                throw;
            }
        }

        public async Task<QuotationStatsDTO> GetQuotationStatsAsync(int userId, string userType)
        {
            try
            {
                return userType.ToLower() switch
                {
                    "customer" => await GetCustomerQuotationStatsAsync(userId),
                    "distributor" => await GetDistributorQuotationStatsAsync(userId),
                    "admin" => await GetAdminQuotationStatsAsync(),
                    _ => new QuotationStatsDTO()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting quotation stats for {userType} {userId} at 2025-07-31 09:12:52 UTC by leshancha");
                return new QuotationStatsDTO();
            }
        }

        private async Task<QuotationStatsDTO> GetCustomerQuotationStatsAsync(int customerId)
        {
            var requests = await _context.QuotationRequests
                .Include(qr => qr.Responses)
                .Where(qr => qr.CustomerId == customerId)
                .ToListAsync();

            return new QuotationStatsDTO
            {
                TotalRequests = requests.Count,
                PendingRequests = requests.Count(qr => qr.Status == "Pending"),
                CompletedRequests = requests.Count(qr => qr.Status == "Completed"),
                CancelledRequests = requests.Count(qr => qr.Status == "Cancelled"),
                TotalResponses = requests.Sum(qr => qr.Responses.Count),
                AverageResponsesPerRequest = requests.Any() ? requests.Average(qr => qr.Responses.Count) : 0,
                LastRequestDate = requests.Any() ? requests.Max(qr => qr.RequestDate) : null
            };
        }

        private async Task<QuotationStatsDTO> GetDistributorQuotationStatsAsync(int distributorId)
        {
            var responses = await _context.QuotationResponses
                .Where(qr => qr.DistributorId == distributorId)
                .ToListAsync();

            return new QuotationStatsDTO
            {
                TotalResponses = responses.Count,
                AcceptedResponses = responses.Count(qr => qr.Status == "Accepted"),
                RejectedResponses = responses.Count(qr => qr.Status == "Rejected"),
                PendingResponses = responses.Count(qr => qr.Status == "Submitted"),
                AverageResponseValue = responses.Any() ? responses.Average(qr => qr.TotalPrice) : 0,
                TotalResponseValue = responses.Sum(qr => qr.TotalPrice),
                LastResponseDate = responses.Any() ? responses.Max(qr => qr.SubmissionDate) : null
            };
        }

        private async Task<QuotationStatsDTO> GetAdminQuotationStatsAsync()
        {
            var allRequests = await _context.QuotationRequests.ToListAsync();
            var allResponses = await _context.QuotationResponses.ToListAsync();

            return new QuotationStatsDTO
            {
                TotalRequests = allRequests.Count,
                TotalResponses = allResponses.Count,
                CompletedRequests = allRequests.Count(qr => qr.Status == "Completed"),
                PendingRequests = allRequests.Count(qr => qr.Status == "Pending"),
                CancelledRequests = allRequests.Count(qr => qr.Status == "Cancelled"),
                AcceptedResponses = allResponses.Count(qr => qr.Status == "Accepted"),
                AverageResponsesPerRequest = allRequests.Any() ? (double)allResponses.Count / allRequests.Count : 0,
                TotalResponseValue = allResponses.Sum(qr => qr.TotalPrice)
            };
        }

        public async Task<List<QuotationRequestItemDTO>> GetQuotationRequestItemsAsync(int requestId)
        {
            return await _context.QuotationRequestItems
                .Include(qri => qri.Product)
                    .ThenInclude(p => p.Category)
                .Where(qri => qri.QuotationRequestId == requestId)
                .Select(qri => new QuotationRequestItemDTO
                {
                    Id = qri.Id,
                    QuotationRequestId = qri.QuotationRequestId,
                    ProductId = qri.ProductId,
                    ProductName = qri.Product.Name,
                    ProductBrand = qri.Product.Brand,
                    ProductCategory = qri.Product.Category.Name,
                    ProductImage = qri.Product.ImageUrl,
                    Quantity = qri.Quantity,
                    Specifications = qri.Specifications
                })
                .ToListAsync();
        }

        public async Task<QuotationResponseDTO?> GetQuotationResponseByIdAsync(int responseId)
        {
            return await _context.QuotationResponses
                .Include(qr => qr.Distributor)
                .Include(qr => qr.Items)
                .Where(qr => qr.Id == responseId)
                .Select(qr => new QuotationResponseDTO
                {
                    Id = qr.Id,
                    QuotationRequestId = qr.QuotationRequestId,
                    DistributorId = qr.DistributorId,
                    DistributorName = qr.Distributor.CompanyName,
                    DistributorEmail = qr.Distributor.Email,
                    DistributorPhone = qr.Distributor.Phone,
                    TotalPrice = qr.TotalPrice,
                    SubmissionDate = qr.SubmissionDate,
                    Status = qr.Status,
                    Notes = qr.Notes,
                    ItemCount = qr.Items.Count,
                    AverageDeliveryDays = (int)qr.Items.Average(qri => qri.DeliveryDays)
                })
                .FirstOrDefaultAsync();
        }

        // ? ADDED: Missing methods for distributor response viewing and editing
        public async Task<QuotationResponseDTO?> GetDistributorQuotationResponseAsync(int requestId, int distributorId)
        {
            try
            {
                return await _context.QuotationResponses
                    .Include(qr => qr.Distributor)
                    .Include(qr => qr.Items)
                        .ThenInclude(qri => qri.Product)
                    .Where(qr => qr.QuotationRequestId == requestId && qr.DistributorId == distributorId)
                    .Select(qr => new QuotationResponseDTO
                    {
                        Id = qr.Id,
                        QuotationRequestId = qr.QuotationRequestId,
                        DistributorId = qr.DistributorId,
                        DistributorName = qr.Distributor.CompanyName,
                        DistributorEmail = qr.Distributor.Email,
                        DistributorPhone = qr.Distributor.Phone,
                        TotalPrice = qr.TotalPrice,
                        SubmissionDate = qr.SubmissionDate,
                        Status = qr.Status,
                        Notes = qr.Notes,
                        ItemCount = qr.Items.Count,
                        AverageDeliveryDays = qr.Items.Any() ? (int)qr.Items.Average(qri => qri.DeliveryDays) : 0,
                        Items = qr.Items.Any() ? qr.Items.Select(qri => new QuotationResponseItemDTO
                        {
                            Id = qri.Id,
                            QuotationResponseId = qri.QuotationResponseId,
                            ProductId = qri.ProductId,
                            ProductName = qri.Product.Name,
                            ProductBrand = qri.Product.Brand,
                            ProductImage = qri.Product.ImageUrl,
                            UnitPrice = qri.UnitPrice,
                            Stock = qri.Stock,
                            DeliveryDays = qri.DeliveryDays,
                            Quantity = qri.Quantity,
                            TotalPrice = qri.TotalPrice
                        }).ToList() : new List<QuotationResponseItemDTO>()
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation response for request {requestId}, distributor {distributorId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return null;
            }
        }

        public async Task<QuotationResponseDTO> UpdateQuotationResponseAsync(int responseId, CreateQuotationResponseDTO updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingResponse = await _context.QuotationResponses
                    .Include(qr => qr.Items)
                    .FirstOrDefaultAsync(qr => qr.Id == responseId);

                if (existingResponse == null)
                {
                    throw new InvalidOperationException("Quotation response not found");
                }

                if (existingResponse.Status != "Submitted")
                {
                    throw new InvalidOperationException($"Cannot update quotation response with status: {existingResponse.Status}");
                }

                // Update basic response details
                existingResponse.Notes = updateDto.Notes ?? existingResponse.Notes;
                existingResponse.SubmissionDate = DateTime.UtcNow; // Update submission time

                // Remove existing items
                _context.QuotationResponseItems.RemoveRange(existingResponse.Items);
                await _context.SaveChangesAsync();

                // Validate new ProductIds
                var requestedProductIds = updateDto.Items.Select(i => i.ProductId).Distinct().ToList();
                var existingProducts = await _context.Products
                    .Where(p => requestedProductIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var missingProductIds = requestedProductIds.Except(existingProducts).ToList();
                if (missingProductIds.Any())
                {
                    var errorMessage = $"The following ProductIds do not exist in the database: {string.Join(", ", missingProductIds)}";
                    _logger.LogError($"? Foreign key constraint would fail: {errorMessage}");
                    throw new InvalidOperationException(errorMessage);
                }

                // Add new items
                decimal totalPrice = 0;
                var newResponseItems = new List<QuotationResponseItem>();

                foreach (var itemDto in updateDto.Items)
                {
                    if (!existingProducts.Contains(itemDto.ProductId))
                    {
                        _logger.LogWarning($"?? Skipping invalid ProductId {itemDto.ProductId} in quotation response update");
                        continue;
                    }

                    var responseItem = new QuotationResponseItem
                    {
                        QuotationResponseId = existingResponse.Id,
                        ProductId = itemDto.ProductId,
                        UnitPrice = itemDto.UnitPrice,
                        Stock = itemDto.Stock,
                        DeliveryDays = itemDto.DeliveryDays,
                        Quantity = itemDto.Quantity,
                        TotalPrice = itemDto.UnitPrice * itemDto.Quantity
                    };

                    newResponseItems.Add(responseItem);
                    totalPrice += responseItem.TotalPrice;
                }

                if (!newResponseItems.Any())
                {
                    throw new InvalidOperationException("No valid response items were created. All ProductIds were invalid.");
                }

                existingResponse.TotalPrice = totalPrice;
                _context.QuotationResponseItems.AddRange(newResponseItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"? Quotation response {responseId} updated successfully by distributor {existingResponse.DistributorId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                return await GetQuotationResponseByIdAsync(responseId) ??
                    throw new InvalidOperationException("Failed to retrieve updated quotation response");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating quotation response {responseId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                throw;
            }
        }

        public async Task<bool> HasDistributorRespondedAsync(int requestId, int distributorId)
        {
            try
            {
                return await _context.QuotationResponses
                    .AnyAsync(qr => qr.QuotationRequestId == requestId && qr.DistributorId == distributorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if distributor {distributorId} has responded to request {requestId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                return false;
            }
        }
    }
}