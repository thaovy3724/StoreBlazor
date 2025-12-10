using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using System.Threading.Tasks;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface IAdminPersonalInfoService
    {
        Task<AdminPersonalInfoDTO?> GetPersonalInfoAsync(int adminId);
        Task<ServiceResult> UpdatePersonalInfoAsync(int adminId, AdminPersonalInfoDTO dto);
        Task<ServiceResult> ChangePasswordAsync(int adminId, AdminChangePasswordDTO dto);

    }
}
