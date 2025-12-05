using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IRegisterService
    {
        Task<ServiceResult> RegisterAsync(RegisterDTO registerDTO);
    }
}