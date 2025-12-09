using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;

namespace StoreBlazor.Services
{
    public interface IAuthService
    {
        Task<ServiceResult> LoginAsync(LoginDTO loginDTO, string userType);
        Task<ServiceResult> LogoutAsync(string? username);
        Task<UserResponseDTO?> GetCurrentUserAsync(string email);
    }
}