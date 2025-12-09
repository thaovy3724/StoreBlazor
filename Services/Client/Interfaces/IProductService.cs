using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IProductService
    {
        Task<PageResult<ProductCardDTO>> GetAllProductAsync(int page);
        Task<PageResult<ProductCardDTO>> FilterAsync(string keyword, int category, decimal? priceFrom, decimal? priceTo, int page);

    }
}
