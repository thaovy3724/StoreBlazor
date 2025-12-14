using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class AdminPersonalInfoService : IAdminPersonalInfoService
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminPersonalInfoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AdminPersonalInfoDTO?> GetPersonalInfoAsync(int adminId)
        {
            if (adminId <= 0) return null;

            var admin = await _dbContext.Users.FindAsync(adminId);
            if (admin == null) return null;

            return new AdminPersonalInfoDTO
            {
                Username = admin.Username ?? "",
                FullName = admin.FullName ?? "",
                Role = admin.Role.ToString()
            };
        }

        public async Task<ServiceResult> UpdatePersonalInfoAsync(int adminId, AdminPersonalInfoDTO dto)
        {
            if (adminId <= 0)
                return new ServiceResult { Type = "error", Message = "Bạn chưa đăng nhập!" };

            var admin = await _dbContext.Users.FindAsync(adminId);
            if (admin == null)
                return new ServiceResult { Type = "error", Message = "Không tìm thấy tài khoản admin." };

            // Kiểm tra trùng username (trừ chính user hiện tại)
            var newUsername = (dto.Username ?? string.Empty).Trim();
            if (!string.Equals(admin.Username, newUsername, StringComparison.OrdinalIgnoreCase))
            {
                var existed = await _dbContext.Users
                    .AnyAsync(u => u.Username == newUsername && u.UserId != adminId);
                if (existed)
                {
                    return new ServiceResult { Type = "error", Message = "Tên đăng nhập đã tồn tại, vui lòng chọn tên khác!" };
                }
            }

            // 2) Kiểm tra username ở Users KHÔNG được trùng với email ở Customers
            var existedInCustomerEmail = await _dbContext.Customers
                .AnyAsync(c => c.Email != null && c.Email.ToLower() == newUsername);

            if (existedInCustomerEmail)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên đăng nhập không hợp lệ (trùng email khách hàng). Vui lòng chọn tên khác!"
                };
            }

            try
            {
                admin.FullName = (dto.FullName ?? string.Empty).Trim();
                admin.Username = newUsername;
                await _dbContext.SaveChangesAsync();

                return new ServiceResult { Type = "success", Message = "Cập nhật thông tin thành công!" };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Type = "error", Message = $"Có lỗi xảy ra: {ex.Message}" };
            }
        }


        public async Task<ServiceResult> ChangePasswordAsync(int adminId, AdminChangePasswordDTO dto)
        {
            if (adminId <= 0)
                return new ServiceResult { Type = "error", Message = "Bạn chưa đăng nhập!" };

            var admin = await _dbContext.Users.FindAsync(adminId);
            if (admin == null)
                return new ServiceResult { Type = "error", Message = "Không tìm thấy tài khoản admin!" };

            // Kiểm tra xác nhận mật khẩu mới
            if (dto.NewPassword != dto.ConfirmPassword)
                return new ServiceResult { Type = "error", Message = "Xác nhận mật khẩu không khớp!" };

            // Kiểm tra mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, admin.Password))
                return new ServiceResult { Type = "error", Message = "Mật khẩu hiện tại không đúng!" };

            try
            {
                admin.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _dbContext.SaveChangesAsync();

                return new ServiceResult { Type = "success", Message = "Đổi mật khẩu thành công!" };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Type = "error", Message = $"Có lỗi xảy ra: {ex.Message}" };
            }
        }
    }
}
