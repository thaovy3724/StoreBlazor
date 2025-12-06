using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface ICustomerService
    {
        Task<PageResult<Customer>> GetAllCustomerAsync(int page);
        Task<ServiceResult> CreateAsync(Customer model);
        Task<ServiceResult> UpdateAsync(Customer model);
        Task<ServiceResult> DeleteAsync(Customer model);
        Task<PageResult<Customer>> SearchByNameAsync(string keyword, int page);
    }
}
