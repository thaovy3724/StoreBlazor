using StoreBlazor.DTO.Client;
using StoreBlazor.Models;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductCardDTO>> GetAllProductAsync();
    }
}
