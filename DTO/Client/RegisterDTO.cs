using System.ComponentModel.DataAnnotations;

namespace StoreBlazor.DTO.Client
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Họ tên không được để trống!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự!")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng!")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống!")]
            
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống!")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự!")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống!")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp!")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống!")]
        public string Address { get; set; } = string.Empty;
    }
}