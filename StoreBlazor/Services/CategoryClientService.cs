using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public class CategoryClientService : BaseService, ICategoryClientService
    {
        public CategoryClientService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Category>> GetAllCategoryAsync()
        {
            var list = await _dbContext.Categories
                                   .OrderByDescending(x => x.CategoryId)
                                    .ToListAsync();
            return list;
        }
    }
}
