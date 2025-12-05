using System.ComponentModel.DataAnnotations;

namespace StoreBlazor.DTO.Client
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Họ và tên không được để trống!")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống!")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại không hợp lệ!")]
        public string Phone { get; set; }



        [Required(ErrorMessage = "Email không được để trống!")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống!")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp!")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống!")]
        public string Address { get; set; }
    }
}