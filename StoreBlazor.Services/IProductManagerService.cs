using Microsoft.AspNetCore.Components.Forms;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.Product;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public interface IProductManagerService
    {
        Task<PageResult<Product>> GetAllProductAsync(int page);
        Task<ServiceResult> CreateAsync(ProductFormDto model);
        Task<ServiceResult> UpdateAsync(ProductFormDto model);
        Task<ServiceResult> DeleteAsync(ProductFormDto model);
        Task<string?> UploadImageAsync(IBrowserFile file);
        Task<ProductFormDto?> GetProductDetailAsync(int productId);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Supplier>> GetAllSuppliersAsync();
        Task<PageResult<Product>> FilterAsync(string keyword, int categoryId, decimal? priceFrom, decimal? priceTo, int page);

    }
}
