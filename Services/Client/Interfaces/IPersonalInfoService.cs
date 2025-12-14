using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IPersonalInfoService
    {
        Task<PersonalInfoDTO?> GetPersonalInfoAsync(string email);
        Task<ServiceResult> UpdatePersonalInfoAsync(PersonalInfoDTO dto);
        Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDTO dto);
    }
}
