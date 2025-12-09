using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public class CategoryService : BasePaginationService, ICategoryService
    {
        public CategoryService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public Task<PageResult<Category>> GetAllCategoryAsync(int page)
        {
            var query = _dbContext.Categories
                        .OrderByDescending(x => x.CategoryId);
            return GetPagedAsync<Category>(query, page);
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
            // 1. Tìm thể loại cần xóa
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

            // 2. Kiểm tra xem có Product nào đang dùng Category này không
            bool hasProducts = await _dbContext.Products
                .AnyAsync(p => p.CategoryId == entity.CategoryId);

            if (hasProducts)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Không thể xóa! Vì vẫn còn sản phẩm thuộc loại này."
                };
            }

            // 3. Xóa nếu không bị khóa ngoại
            _dbContext.Categories.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Xóa loại sản phẩm thành công!"
            };
        }


        public Task<PageResult<Category>> SearchByNameAsync(string keyword, int page)
        {
            var kw = keyword.Trim().ToLower();

            var query = _dbContext.Categories
                .Where(x => x.CategoryName.ToLower().Contains(kw))
                .OrderBy(x => x.CategoryId);
            return GetPagedAsync<Category>(query, page); 
        }
    }

}
