using GadgetHubAPI.DTOs;

namespace GadgetHubAPI.Services
{
    public interface IQuotationService
    {
        Task<List<QuotationRequestDTO>> GetCustomerQuotationRequestsAsync(int customerId);
        Task<List<QuotationRequestDTO>> GetDistributorQuotationRequestsAsync(int distributorId);
        Task<QuotationRequestDTO?> GetQuotationRequestByIdAsync(int requestId);
        Task<QuotationRequestDTO> CreateQuotationRequestAsync(CreateQuotationRequestDTO createDto);
        Task<QuotationResponseDTO> SubmitQuotationResponseAsync(CreateQuotationResponseDTO responseDto);
        Task<QuotationResponseDTO?> GetQuotationResponseByIdAsync(int responseId);
        Task<List<QuotationResponseDTO>> GetQuotationResponsesAsync(int requestId);
        Task<QuotationComparisonDTO> GetQuotationComparisonAsync(int requestId);
        Task<OrderDTO> AcceptQuotationAsync(int responseId, int customerId);
        Task<bool> CancelQuotationRequestAsync(int requestId, int customerId);
        Task<QuotationStatsDTO> GetQuotationStatsAsync(int userId, string userType);
        Task<List<QuotationRequestItemDTO>> GetQuotationRequestItemsAsync(int requestId);
        
        // ADDED: Missing methods for distributor response viewing and editing
        Task<QuotationResponseDTO?> GetDistributorQuotationResponseAsync(int requestId, int distributorId);
        Task<QuotationResponseDTO> UpdateQuotationResponseAsync(int responseId, CreateQuotationResponseDTO updateDto);
        Task<bool> HasDistributorRespondedAsync(int requestId, int distributorId);
    }
}