using System.ComponentModel.DataAnnotations;

namespace OutfitShop.DTOs
{
    public class OrderDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận.")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự.")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ.")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng chi tiết.")]
        public string Address { get; set; } = string.Empty;

        public string? OrderNotes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}