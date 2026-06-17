using System.ComponentModel.DataAnnotations;

namespace OutfitShop.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không trùng khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}