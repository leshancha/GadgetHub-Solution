using GadgetHubAPI.DTOs;

namespace GadgetHubAPI.Services
{
    public interface IAdminService
    {
        Task<List<CustomerDTO>> GetAllCustomersAsync();
        Task<List<DistributorDTO>> GetAllDistributorsAsync();
        Task<SystemOverviewDTO> GetSystemOverviewAsync();
        Task<bool> DeactivateCustomerAsync(int customerId);
        Task<bool> ActivateCustomerAsync(int customerId);
        Task<bool> DeactivateDistributorAsync(int distributorId);
        Task<bool> ActivateDistributorAsync(int distributorId);
        Task<List<OrderDTO>> GetAllOrdersAsync();
        Task<List<QuotationRequestDTO>> GetAllQuotationRequestsAsync();
        Task<DashboardStatsDTO> GetDashboardStatsAsync();
        Task<AdminReportDTO> GenerateSystemReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}