using StoreBlazor.Models;

namespace StoreBlazor.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoryAsync();
    }
}
