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
        public async Task<PersonalInfoDTO?> GetPersonalInfoAsync(int customerId)
        {
            if (customerId <= 0) return null;

            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null) return null;

            return new PersonalInfoDTO
            {
                FullName = customer.Name ?? "",
                Email = customer.Email ?? "",
                Phone = customer.Phone ?? "",
                Address = customer.Address ?? ""
            };
        }

        // Cập nhật thông tin cá nhân
        public async Task<ServiceResult> UpdatePersonalInfoAsync(int customerId, PersonalInfoDTO dto)
        {
            if (customerId <= 0)
                return new ServiceResult { Type = "error", Message = "Bạn chưa đăng nhập!" };

            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null)
                return new ServiceResult { Type = "error", Message = "Không tìm thấy tài khoản!" };

            try
            {
                customer.Name = dto.FullName.Trim();
                customer.Phone = dto.Phone?.Trim();
                customer.Address = dto.Address?.Trim();

                await _dbContext.SaveChangesAsync();

                return new ServiceResult { Type = "success", Message = "Cập nhật thông tin thành công!" };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Type = "error", Message = $"Có lỗi xảy ra: {ex.Message}" };
            }
        }

        // Đổi mật khẩu
        public async Task<ServiceResult> ChangePasswordAsync(int customerId, ChangePasswordDTO dto)
        {
            if (customerId <= 0)
                return new ServiceResult { Type = "error", Message = "Bạn chưa đăng nhập!" };

            try
            {
                // Lấy customer để biết email
                var customer = await _dbContext.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return new ServiceResult { Type = "error", Message = "Không tìm thấy tài khoản!" };
                }

                if (string.IsNullOrWhiteSpace(customer.Email))
                {
                    return new ServiceResult { Type = "error", Message = "Tài khoản không có email hợp lệ!" };
                }

                Console.WriteLine($"Email tìm kiếm: {customer.Email}");

                // Tìm user theo email (username = email)
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == customer.Email);

                if (user == null)
                {
#if DEBUG
                    Console.WriteLine("Không tìm thấy user, đang tạo user mới...");

                    user = new User
                    {
                        Username = customer.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"), // Mật khẩu mặc định cho test
                        FullName = customer.Name,
                        // Role = Role.Customer,
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.Users.Add(user);
                    await _dbContext.SaveChangesAsync();

                    Console.WriteLine("Đã tạo user mới");
#else
                    return new ServiceResult { Type = "error", Message = "Không tìm thấy tài khoản!" };
#endif
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
