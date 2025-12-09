using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Services.Client.Interfaces;

namespace StoreBlazor.Services.Client.Implementations
{
    public class ProductService : BasePaginationClientService, IProductService
    {
        public ProductService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PageResult<ProductCardDTO>> GetAllProductAsync(int page)
        {
            var query = _dbContext.Products
                        .Include(p => p.Inventory)
                        .Where(p => p.Inventory.Quantity > 0)
                        .OrderBy(p => p.Price)
                        .Select(p => new ProductCardDTO
                        {
                            ProductId = p.ProductId,
                            ProductName = p.ProductName,
                            ProductImage = p.ProductImage,
                            Price = p.Price,
                            Unit = p.Unit,
                            CategoryId = p.CategoryId,
                            Quantity = p.Inventory.Quantity
                        });

            return await GetPagedAsync(query, page);
        }

        public Task<PageResult<ProductCardDTO>> FilterAsync(string keyword, int category, decimal? priceFrom, decimal? priceTo, int page)
        {
            var query = _dbContext.Products
                        .Include(p => p.Inventory)
                        .Where(data => data.Inventory.Quantity > 0)
                        .AsQueryable();

            // ProductName
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.ProductName.Contains(keyword));
            }

            if (category != -1)
            {
                query = query.Where(p=> p.CategoryId == category);
            }

            if (priceFrom.HasValue)
            {
                query = query.Where(p => p.Price >= priceFrom.Value);
            }

            if (priceTo.HasValue)
            {
                query = query.Where(p => p.Price <= priceTo.Value);
            }

            query = query.OrderBy(p=> p.Price);

            var dtoQuery = query.Select(p => new ProductCardDTO
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductImage = p.ProductImage,
                Price = p.Price,
                Unit = p.Unit,
                CategoryId = p.CategoryId,
                Quantity = p.Inventory.Quantity
            });
            return GetPagedAsync<ProductCardDTO>(dtoQuery, page);
        }
    }
}
