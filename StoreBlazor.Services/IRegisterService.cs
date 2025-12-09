using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;

namespace StoreBlazor.Services
{
    public interface IRegisterService
    {
        Task<ServiceResult> RegisterAsync(RegisterDTO registerDTO);
    }
}