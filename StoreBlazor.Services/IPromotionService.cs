using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public interface IPromotionService
    {
        Task<PageResult<Promotion>> GetAllPromotionAsync(int page);
        Task<ServiceResult> CreateAsync(Promotion model);
        Task<ServiceResult> UpdateAsync(Promotion model);
        Task<ServiceResult> LockAsync(Promotion model);
        Task<PageResult<Promotion>> FilterAsync(string keyword, int status, DateTime? fromDate, DateTime? toDate, int page);

    }
}
