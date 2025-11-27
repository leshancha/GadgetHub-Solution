using GadgetHubAPI.DTOs;

namespace GadgetHubAPI.Services
{
    public interface IAuthService
    {
        Task<string> GenerateJwtTokenAsync(string userType, int userId, string name, string email);
        Task<LoginResponseDTO?> LoginAsync(string email, string password, string userType);
        Task<LoginResponseDTO?> RegisterCustomerAsync(RegisterCustomerDTO registerDto);
        Task<LoginResponseDTO?> RegisterDistributorAsync(RegisterDistributorDTO registerDto);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> EmailExistsAsync(string email);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}