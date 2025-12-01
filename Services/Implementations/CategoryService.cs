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
            var list = await _dbContext.Categories
                            .OrderBy(c => c.CategoryId)
                            .ToListAsync(); // dòng này mới query dữ liệu

            return list;
        }

        public async Task<ServiceResult> CreateAsync(Category item)
        {
            // Kiểm tra trùng tên
            var existingCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == item.CategoryName.ToLower());

            if (existingCategory != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên loại sản phẩm đã tồn tại!"
                };
            }

            _dbContext.Categories.Add(item);
            await _dbContext.SaveChangesAsync();
            
            return new ServiceResult
            {
                Type = "success",
                Message = "Thêm loại sản phẩm thành công!"
            };
        }

        public async Task<ServiceResult> UpdateAsync(Category item)
        {
            // 1. Tìm thể loại cần cập nhật
            var entity = await _dbContext.Categories
                .FirstOrDefaultAsync(x => x.CategoryId == item.CategoryId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Thể loại không tồn tại!"
                };
            }

            // 2. Kiểm tra trùng tên (trừ chính nó ra)
            var newName = item.CategoryName.Trim();
            var existingCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId != item.CategoryId &&
                    c.CategoryName.ToLower() == newName.ToLower());

            if (existingCategory != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên loại sản phẩm đã tồn tại!"
                };
            }

            // 3. Cập nhật
            entity.CategoryName = newName;

            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Cập nhật loại sản phẩm thành công!"
            };
        }

        public async Task<ServiceResult> DeleteAsync(Category item)
        {
            // 1. Tìm thể loại cần cập nhật
            var entity = await _dbContext.Categories
                .FirstOrDefaultAsync(x => x.CategoryId == item.CategoryId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Thể loại không tồn tại!"
                };
            }

            _dbContext.Categories.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return new ServiceResult
            {
                Type = "success",
                Message = "Xóa loại sản phẩm thành công!"
            };
        }

        public async Task<List<Category>> SearchByNameAsync(string keyword)
        {
            // Tìm thể loại theo keyword (không phân biệt hoa thường)
            var kw = keyword.Trim().ToLower();

            return await _dbContext.Categories
                .Where(x => x.CategoryName.ToLower().Contains(kw))   // ✅ không phân biệt hoa thường
                .OrderBy(x => x.CategoryId)
                .ToListAsync();
        }

    }

}
