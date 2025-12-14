using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class CustomerService : BasePaginationService, ICustomerService
    {
        public CustomerService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public Task<PageResult<Customer>> GetAllCustomerAsync(int page)
        {
            var query = _dbContext.Customers
                        .OrderByDescending(x => x.CustomerId);
            return GetPagedAsync<Customer>(query, page);
        }

        public async Task<ServiceResult> CreateAsync(Customer model)
        {
            // Kiểm tra trùng email (nếu có email)
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingCustomer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == model.Email.ToLower());

                if (existingCustomer != null)
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
                var existingPhone = await _dbContext.Customers
                    .Where(c => c.Phone == model.Phone)
                    .Where(c =>
                        !_dbContext.Users
                            .Any(u => u.Username == c.Email)
                    )
                    .FirstOrDefaultAsync();

                if (existingPhone != null)
                {
                    return new ServiceResult
                    {
                        Type = "error",
                        Message = "Số điện thoại này đã tồn tại!"
                    };
                }
            }


            model.CreatedAt = DateTime.Now;
            _dbContext.Customers.Add(model);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Thêm khách hàng thành công!"
            };
        }

        public async Task<ServiceResult> UpdateAsync(Customer model)
        {
            // Tìm khách hàng cần cập nhật
            var entity = await _dbContext.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == model.CustomerId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Khách hàng không tồn tại!"
                };
            }

            // Kiểm tra trùng email (trừ chính nó ra)
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingCustomer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c =>
                        c.CustomerId != model.CustomerId &&
                        c.Email.ToLower() == model.Email.ToLower());

                if (existingCustomer != null)
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
                var existingPhone = await _dbContext.Customers
                    .Where(c =>
                        c.CustomerId != model.CustomerId &&      // loại trừ chính nó
                        c.Phone == model.Phone                   // cùng số điện thoại
                    )
                    .Where(c =>
                        !_dbContext.Users
                            .Any(u => u.Username == c.Email)     // loại trừ khách có email = username
                    )
                    .FirstOrDefaultAsync();

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
            entity.Name = model.Name.Trim();
            entity.Phone = model.Phone?.Trim();
            entity.Email = model.Email?.Trim();
            entity.Address = model.Address?.Trim();

            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Cập nhật khách hàng thành công!"
            };
        }

        public async Task<ServiceResult> DeleteAsync(Customer model)
        {
            // Tìm khách hàng cần xóa
            var entity = await _dbContext.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == model.CustomerId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Khách hàng không tồn tại!"
                };
            }

            // Kiểm tra xem có đơn hàng nào đang liên kết với khách hàng này không
            bool hasOrders = await _dbContext.Orders
                .AnyAsync(o => o.CustomerId == entity.CustomerId);

            if (hasOrders)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Không thể xóa! Vì vẫn còn đơn hàng thuộc khách hàng này."
                };
            }

            _dbContext.Customers.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Xóa khách hàng thành công!"
            };
        }

        public Task<PageResult<Customer>> SearchByNameAsync(string keyword, int page)
        {
            var kw = keyword.Trim().ToLower();

            var query = _dbContext.Customers
                .Where(x => x.Name.ToLower().Contains(kw) ||
                           (x.Email != null && x.Email.ToLower().Contains(kw)) ||
                           (x.Phone != null && x.Phone.Contains(kw)))
                .OrderBy(x => x.CustomerId);
            return GetPagedAsync<Customer>(query, page);
        }
    }
}
