using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.OrderManager;
using StoreBlazor.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StoreBlazor.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<List<Promotion>> GetAllPromotionAsync();
        Task<ServiceResult> CreateAsync(Promotion model);
        Task<ServiceResult> UpdateAsync(Promotion model);
        Task<ServiceResult> LockAsync(Promotion model);

    }
}
