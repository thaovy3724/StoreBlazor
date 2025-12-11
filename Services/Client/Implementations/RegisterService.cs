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
            // Kiểm tra trùng email trong cả 2 bảng Users và Customers
            var emailLower = registerDTO.Email.ToLower();

            var existingUser = await _dbContext.Users
                .AnyAsync(u => u.Username.ToLower() == emailLower);

            var existingCustomer = await _dbContext.Customers
                .AnyAsync(c => c.Email.ToLower() == emailLower);

            if (existingUser || existingCustomer)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Email này đã được đăng ký!"
                };
            }


            string hashedPassword = await Task.Run(() =>
                BCrypt.Net.BCrypt.HashPassword(registerDTO.Password, workFactor: 8)
            );

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Tạo Customer mới
                var customer = new Customer
                {
                    Name = registerDTO.Name.Trim(),
                    Phone = registerDTO.Phone.Trim(),
                    Email = registerDTO.Email.Trim(),
                    Address = registerDTO.Address?.Trim() ?? "",
                    CreatedAt = DateTime.Now
                };

                _dbContext.Customers.Add(customer);

                // Tạo User mới
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

                await transaction.CommitAsync();

                return new ServiceResult
                {
                    Type = "success",
                    Message = "Đăng ký tài khoản thành công!"
                };
            }
            catch (Exception ex)
            {
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