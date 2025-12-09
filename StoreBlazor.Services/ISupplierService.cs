using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public interface ISupplierService
    {
        Task<PageResult<Supplier>> GetAllSupplierAsync(int page);
        Task<ServiceResult> CreateAsync(Supplier model);
        Task<ServiceResult> UpdateAsync(Supplier model);
        Task<ServiceResult> DeleteAsync(Supplier model);
        Task<PageResult<Supplier>> SearchByNameAsync(string keyword, int page);
    }
}

