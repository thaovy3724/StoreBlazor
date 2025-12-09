using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public interface ICategoryClientService
    {
        Task<List<Category>> GetAllCategoryAsync();

    }
}
