using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StoreBlazor.DTO.Client
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

    public class PersonalInfoDTO
    {
        [ValidFullName]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống!")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống!")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại không hợp lệ!")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống!")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự!")]
        public string Address { get; set; } = string.Empty;
    }

    public class ChangePasswordDTO
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
}
