using System.ComponentModel.DataAnnotations;

namespace OutfitShop.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}