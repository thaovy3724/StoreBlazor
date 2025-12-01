using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;
using StoreBlazor.Services.Interfaces;

namespace StoreBlazor.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _dbContext;
        public CategoryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Category>> GetAllCategoryAsync()
        {
            // Hàm chứa await → bắt buộc phải trả về Task<...>
            // có await thì method phải là async
            // Defer execution: chưa query dữ liệu ngay mà phải đợi đến khi nào có await thì mới query dữ liệu
            var query = _dbContext.Categories.AsQueryable();
            var list = await query
            .OrderBy(c => c.CategoryId)
            .ToListAsync(); // dòng này mới query dữ liệu

            return list;
        }
    }
}
