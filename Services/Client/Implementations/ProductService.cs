using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;

namespace StoreBlazor.Services.Client.Implementations
{
    public class ProductService : BaseService, IProductService
    {
        public ProductService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<ProductCardDTO>> GetAllProductAsync()
        {
            var list = await _dbContext.Products
                        .Join(
                            _dbContext.Inventories,
                            product => product.ProductId,
                            inventory => inventory.ProductId,
                            (product, inventory) => new
                            {
                                Product = product,
                                Inventory = inventory
                            }
                        )
                        .Where(data => data.Inventory.Quantity > 0)
                        .Select(data => new ProductCardDTO
                        {
                            Product = data.Product,
                            Quantity = data.Inventory.Quantity
                        })
                        .ToListAsync();

            return list;
        }
    }
}
