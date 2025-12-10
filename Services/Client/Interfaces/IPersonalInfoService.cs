using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IPersonalInfoService
    {
        Task<PersonalInfoDTO?> GetPersonalInfoAsync(int customerId);
        Task<ServiceResult> UpdatePersonalInfoAsync(int customerId, PersonalInfoDTO dto);
        Task<ServiceResult> ChangePasswordAsync(int customerId, ChangePasswordDTO dto);
    }
}
