using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StoreBlazor.DTO.Admin
{
    // Custom validation cho họ tên
    public class ValidFullNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var fullName = value as string ?? string.Empty;
            var trimmedName = fullName.Trim();

            if (string.IsNullOrEmpty(trimmedName))
                return new ValidationResult("Họ tên không được để trống!");

            if (trimmedName.Length < 2)
                return new ValidationResult("Họ tên phải có ít nhất 2 ký tự!");

            if (trimmedName.Length > 100)
                return new ValidationResult("Họ tên không được vượt quá 100 ký tự!");

            // Chỉ cho phép chữ cái và khoảng trắng 
            if (!Regex.IsMatch(trimmedName, @"^[\p{L}\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant))
                return new ValidationResult("Họ tên chỉ được chứa chữ cái và khoảng trắng!");

            // Không được có nhiều khoảng trắng liên tiếp
            if (Regex.IsMatch(trimmedName, @"\s{2,}"))
                return new ValidationResult("Không được có nhiều khoảng trắng liên tiếp!");

            return ValidationResult.Success;
        }
    }

    // Custom validation cho tên đăng nhập
    public class ValidUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var username = value as string ?? string.Empty;
            var trimmedUsername = username.Trim();

            if (string.IsNullOrEmpty(trimmedUsername))
                return new ValidationResult("Tên đăng nhập không được để trống!");

            if (trimmedUsername.Length < 4)
                return new ValidationResult("Tên đăng nhập phải có ít nhất 4 ký tự!");

            if (trimmedUsername.Length > 50)
                return new ValidationResult("Tên đăng nhập không được vượt quá 50 ký tự!");

            // Chỉ cho phép chữ, số, dấu gạch dưới và khoảng trắng
            if (!Regex.IsMatch(trimmedUsername, @"^[a-zA-Z0-9_ ]+$", RegexOptions.Compiled))
                return new ValidationResult("Tên đăng nhập không được chứa kí tự đặc biệt!");

            return ValidationResult.Success;
        }
    }

    public class AdminChangePasswordDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại!")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới!")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự!")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới!")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp!")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AdminPersonalInfoDTO
    {
        [ValidFullName]
        public string FullName { get; set; } = string.Empty;

        [ValidUsername]
        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }


}
