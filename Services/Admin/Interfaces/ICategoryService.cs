using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface ICategoryService
    {
        Task<PageResult<Category>> GetAllCategoryAsync(int page);
        Task<ServiceResult> CreateAsync(Category model);
        Task<ServiceResult> UpdateAsync(Category model);
        Task<ServiceResult> DeleteAsync(Category model);
        Task<PageResult<Category>> SearchByNameAsync(string keyword, int page);
    }
}
