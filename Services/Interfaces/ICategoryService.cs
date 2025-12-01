using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StoreBlazor.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoryAsync();
        Task<ServiceResult> CreateAsync(Category model);
        Task<ServiceResult> UpdateAsync(Category model);
        Task<ServiceResult> DeleteAsync(Category model);
        Task<List<Category>> SearchByNameAsync(string keyword);
    }
}
