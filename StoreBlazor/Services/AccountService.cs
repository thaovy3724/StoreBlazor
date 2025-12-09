using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public class AccountService : BasePaginationService, IAccountService
    {
        public AccountService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public Task<PageResult<User>> GetAllAccountAsync(int page)
        {
            var query = _dbContext.Users
                        .OrderByDescending(x => x.UserId);
            return GetPagedAsync<User>(query, page);
        }

        public async Task<ServiceResult> CreateAsync(User model)
        {
            // Kiểm tra trùng username
            var existingAccount = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());

            if (existingAccount != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên tài khoản đã tồn tại!"
                };
            }

            // Mã hóa mật khẩu
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            model.CreatedAt = DateTime.Now;

            _dbContext.Users.Add(model);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Thêm tài khoản thành công!"
            };
        }

        public async Task<ServiceResult> UpdateAsync(User model)
        {
            // Tìm tài khoản cần cập nhật
            var entity = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.UserId == model.UserId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tài khoản không tồn tại!"
                };
            }

            // Kiểm tra trùng username (trừ chính nó ra)
            var newUsername = model.Username.Trim();
            var existingAccount = await _dbContext.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId != model.UserId &&
                    u.Username.ToLower() == newUsername.ToLower());

            if (existingAccount != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên tài khoản đã tồn tại!"
                };
            }

            // Cập nhật thông tin
            entity.Username = newUsername;
            entity.FullName = model.FullName?.Trim() ?? "";
            entity.Role = model.Role;

            // Chỉ cập nhật mật khẩu nếu có mật khẩu mới
            if (!string.IsNullOrEmpty(model.Password))
            {
                entity.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Cập nhật tài khoản thành công!"
            };
        }

        public async Task<ServiceResult> DeleteAsync(User model)
        {
            // Tìm tài khoản cần xóa
            var entity = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.UserId == model.UserId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tài khoản không tồn tại!"
                };
            }

            // Kiểm tra xem có đơn hàng nào đang liên kết với tài khoản này không
            bool hasOrders = await _dbContext.Orders
                .AnyAsync(o => o.UserId == entity.UserId);

            if (hasOrders)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Không thể xóa! Vì vẫn còn đơn hàng thuộc tài khoản này."
                };
            }

            _dbContext.Users.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Xóa tài khoản thành công!"
            };
        }

        public Task<PageResult<User>> SearchByNameAsync(string keyword, int page)
        {
            var kw = keyword.Trim().ToLower();

            var query = _dbContext.Users
                .Where(x => x.Username.ToLower().Contains(kw) ||
                           (x.FullName != null && x.FullName.ToLower().Contains(kw)))
                .OrderBy(x => x.UserId);
            return GetPagedAsync<User>(query, page);
        }
    }
}
