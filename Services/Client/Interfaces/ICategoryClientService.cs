using StoreBlazor.Models;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface ICategoryClientService
    {
        Task<List<Category>> GetAllCategoryAsync();

    }
}
