using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;

namespace StoreBlazor.Services.Client.Implementations
{
    public class PersonalInfoService : IPersonalInfoService
    {
        private readonly ApplicationDbContext _dbContext;

        public PersonalInfoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Lấy thông tin cá nhân theo customerId
        public async Task<PersonalInfoDTO?> GetPersonalInfoAsync(string email)
        {
            // Tìm user để lấy role (nếu cần)
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Email == email);
            return new PersonalInfoDTO
            {
                UserId = customer.CustomerId,
                FullName = customer.Name ?? "",
                Email = customer.Email ?? "",
                Phone = customer.Phone ?? "",
                Address = customer.Address ?? ""
            };
        }


        // Cập nhật thông tin cá nhân
        public async Task<ServiceResult> UpdatePersonalInfoAsync(PersonalInfoDTO dto)
        {
            var customer = await _dbContext.Customers.FindAsync(dto.UserId); 
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == dto.Email);

            if (customer == null || user == null)
                return new ServiceResult { Type = "error", Message = "Không tìm thấy tài khoản!" };

            try
            {
                // Cập nhật thông tin trong customer
                customer.Name = dto.FullName.Trim();
                customer.Phone = dto.Phone?.Trim();
                customer.Address = dto.Address?.Trim();

                // Cập nhật thông tin trong user (fullname)
                user.FullName = dto.FullName.Trim();

                await _dbContext.SaveChangesAsync();

                return new ServiceResult { Type = "success", Message = "Cập nhật thông tin thành công!" };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Type = "error", Message = $"Có lỗi xảy ra: {ex.Message}" };
            }
        }

        // Đổi mật khẩu
        public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDTO dto)
        {
            if (userId <= 0)
                return new ServiceResult { Type = "error", Message = "Bạn chưa đăng nhập!" };

            try
            {
                var user = await _dbContext.Users.FindAsync(userId);

                
                if (user == null)
                {
                    return new ServiceResult { Type = "error", Message = "Không tìm thấy tài khoản!" };
                }

                Console.WriteLine("Đang kiểm tra mật khẩu hiện tại...");
                Console.WriteLine($"Mật khẩu nhập vào: {dto.CurrentPassword}");
                Console.WriteLine($"Mật khẩu hash trong DB: {user.Password}");

                // Kiểm tra mật khẩu hiện tại
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                {
                    Console.WriteLine("Mật khẩu không khớp!");
                    return new ServiceResult { Type = "error", Message = "Mật khẩu hiện tại không đúng!" };
                }

                Console.WriteLine("Đang hash mật khẩu mới...");
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                Console.WriteLine("Đang lưu vào database...");
                await _dbContext.SaveChangesAsync();

                Console.WriteLine("Đổi mật khẩu thành công!");
                return new ServiceResult { Type = "success", Message = "Đổi mật khẩu thành công!" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ChangePasswordAsync: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                return new ServiceResult
                {
                    Type = "error",
                    Message = $"Lỗi database: {ex.InnerException?.Message ?? ex.Message}"
                };
            }
        }
    }
}
