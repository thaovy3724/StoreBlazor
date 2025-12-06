using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class SupplierService : BasePaginationService, ISupplierService
    {
        public SupplierService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public Task<PageResult<Supplier>> GetAllSupplierAsync(int page)
        {
            var query = _dbContext.Suppliers
                        .OrderByDescending(x => x.SupplierId);
            return GetPagedAsync<Supplier>(query, page);
        }

        public async Task<ServiceResult> CreateAsync(Supplier model)
        {
            // Kiểm tra trùng tên nhà cung cấp
            var existingSupplier = await _dbContext.Suppliers
                .FirstOrDefaultAsync(s => s.Name.ToLower() == model.Name.ToLower());

            if (existingSupplier != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên nhà cung cấp đã tồn tại!"
                };
            }

            // Kiểm tra trùng email (nếu có email)
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingEmail = await _dbContext.Suppliers
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == model.Email.ToLower());

                if (existingEmail != null)
                {
                    return new ServiceResult
                    {
                        Type = "error",
                        Message = "Email này đã tồn tại!"
                    };
                }
            }

            // Kiểm tra trùng số điện thoại (nếu có)
            if (!string.IsNullOrEmpty(model.Phone))
            {
                var existingPhone = await _dbContext.Suppliers
                    .FirstOrDefaultAsync(s => s.Phone == model.Phone);

                if (existingPhone != null)
                {
                    return new ServiceResult
                    {
                        Type = "error",
                        Message = "Số điện thoại này đã tồn tại!"
                    };
                }
            }

            _dbContext.Suppliers.Add(model);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Thêm nhà cung cấp thành công!"
            };
        }

        public async Task<ServiceResult> UpdateAsync(Supplier model)
        {
            // Tìm nhà cung cấp cần cập nhật
            var entity = await _dbContext.Suppliers
                .FirstOrDefaultAsync(x => x.SupplierId == model.SupplierId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Nhà cung cấp không tồn tại!"
                };
            }

            // Kiểm tra trùng tên (trừ chính nó ra)
            var newName = model.Name.Trim();
            var existingSupplier = await _dbContext.Suppliers
                .FirstOrDefaultAsync(s =>
                    s.SupplierId != model.SupplierId &&
                    s.Name.ToLower() == newName.ToLower());

            if (existingSupplier != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên nhà cung cấp đã tồn tại!"
                };
            }

            // Kiểm tra trùng email (trừ chính nó ra)
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingEmail = await _dbContext.Suppliers
                    .FirstOrDefaultAsync(s =>
                        s.SupplierId != model.SupplierId &&
                        s.Email.ToLower() == model.Email.ToLower());

                if (existingEmail != null)
                {
                    return new ServiceResult
                    {
                        Type = "error",
                        Message = "Email này đã tồn tại!"
                    };
                }
            }

            // Kiểm tra trùng số điện thoại (trừ chính nó ra)
            if (!string.IsNullOrEmpty(model.Phone))
            {
                var existingPhone = await _dbContext.Suppliers
                    .FirstOrDefaultAsync(s =>
                        s.SupplierId != model.SupplierId &&
                        s.Phone == model.Phone);

                if (existingPhone != null)
                {
                    return new ServiceResult
                    {
                        Type = "error",
                        Message = "Số điện thoại này đã tồn tại!"
                    };
                }
            }

            // Cập nhật thông tin
            entity.Name = newName;
            entity.Phone = model.Phone?.Trim();
            entity.Email = model.Email?.Trim();
            entity.Address = model.Address?.Trim();

            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Cập nhật nhà cung cấp thành công!"
            };
        }

        public async Task<ServiceResult> DeleteAsync(Supplier model)
        {
            // Tìm nhà cung cấp cần xóa
            var entity = await _dbContext.Suppliers
                .FirstOrDefaultAsync(x => x.SupplierId == model.SupplierId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Nhà cung cấp không tồn tại!"
                };
            }

            // Kiểm tra xem có sản phẩm nào đang liên kết với nhà cung cấp này không
            bool hasProducts = await _dbContext.Products
                .AnyAsync(p => p.SupplierId == entity.SupplierId);

            if (hasProducts)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Không thể xóa! Vì vẫn còn sản phẩm thuộc nhà cung cấp này."
                };
            }

            _dbContext.Suppliers.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Xóa nhà cung cấp thành công!"
            };
        }

        public Task<PageResult<Supplier>> SearchByNameAsync(string keyword, int page)
        {
            var kw = keyword.Trim().ToLower();

            var query = _dbContext.Suppliers
                .Where(x => x.Name.ToLower().Contains(kw) ||
                           (x.Email != null && x.Email.ToLower().Contains(kw)) ||
                           (x.Phone != null && x.Phone.Contains(kw)))
                .OrderBy(x => x.SupplierId);
            return GetPagedAsync<Supplier>(query, page);
        }
    }
}

