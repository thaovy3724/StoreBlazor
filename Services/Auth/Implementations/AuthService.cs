using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Client;
using StoreBlazor.Models;
using StoreBlazor.Services.Auth.Interfaces;

namespace StoreBlazor.Services.Auth.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> LoginAsync(LoginDTO loginDTO, string userType)
        {
            var username = loginDTO.Email?.Trim() ?? string.Empty;

            // Lấy user từ DB (MySQL mặc định case-insensitive)
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            // So sánh lại trong C# để đảm bảo case-sensitive
            if (user == null || !user.Username.Equals(username, StringComparison.Ordinal))
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng."
                };
            }

            if (userType == "customer" && user.Role != Role.Customer)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tài khoản không phải khách hàng."
                };
            }

            if (userType == "admin" && !(user.Role == Role.Admin || user.Role == Role.Staff))
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tài khoản không có quyền truy cập admin."
                };
            }

            var isValid = BCrypt.Net.BCrypt.Verify(loginDTO.Password ?? string.Empty, user.Password);
            if (!isValid)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng."
                };
            }

            return new ServiceResult
            {
                Type = "success",
                Message = "Đăng nhập thành công."
            };
        }

        public async Task<ServiceResult> LogoutAsync(string? username)
        {
            await Task.CompletedTask;
            return new ServiceResult
            {
                Type = "success",
                Message = "Đăng xuất thành công."
            };
        }

        public async Task<UserResponseDTO?> GetCurrentUserAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var username = email.Trim();

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            // So sánh case-sensitive
            if (user == null || !user.Username.Equals(username, StringComparison.Ordinal))
            {
                return null;
            }

            return new UserResponseDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName ?? user.Username,
                Role = user.Role.ToString()
            };
        }
    }
}