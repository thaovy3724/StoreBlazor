using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.OrderManager;
using StoreBlazor.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface IPromotionService
    {
        Task<PageResult<Promotion>> GetAllPromotionAsync(int page);
        Task<ServiceResult> CreateAsync(Promotion model);
        Task<ServiceResult> UpdateAsync(Promotion model);
        Task<ServiceResult> LockAsync(Promotion model);

    }
}
