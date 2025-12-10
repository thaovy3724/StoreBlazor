using System.Threading.Tasks;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;

public interface IPersonalInfoService
{
    Task<PersonalInfoDTO?> GetPersonalInfoAsync(int customerId);
    Task<ServiceResult> UpdatePersonalInfoAsync(int customerId, PersonalInfoDTO dto);
    Task<ServiceResult> ChangePasswordAsync(int customerId, ChangePasswordDTO dto);
}
