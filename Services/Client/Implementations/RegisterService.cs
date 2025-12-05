using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;

namespace StoreBlazor.Services.Client.Implementations
{
    public class RegisterService : IRegisterService
    {
        private readonly ApplicationDbContext _dbContext;

        public RegisterService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> RegisterAsync(RegisterDTO registerDTO)
        {
            // Kiểm tra trùng email trong bảng Users
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == registerDTO.Email.ToLower());

            if (existingUser != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Email này đã được đăng ký!"
                };
            }

            // Mã hóa mật khẩu (sử dụng BCrypt)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);

            // Bắt đầu transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Tạo Customer mới
                var customer = new Customer
                {
                    Name = registerDTO.Name.Trim(),
                    Phone = registerDTO.Phone.Trim(),
                    Email = registerDTO.Email.Trim(),
                    Address = registerDTO.Address.Trim(),
                    CreatedAt = DateTime.Now
                };

                _dbContext.Customers.Add(customer);
                await _dbContext.SaveChangesAsync();

                // Tạo User mới (username = email, role = Customer)
                var user = new User
                {
                    Username = registerDTO.Email.Trim(),
                    Password = hashedPassword,
                    FullName = registerDTO.Name.Trim(),
                    Role = Role.Customer,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return new ServiceResult
                {
                    Type = "success",
                    Message = "Đăng ký tài khoản thành công!"
                };
            }
            catch (Exception ex)
            {
                // Rollback nếu có lỗi
                await transaction.RollbackAsync();

                return new ServiceResult
                {
                    Type = "error",
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }
    }
}