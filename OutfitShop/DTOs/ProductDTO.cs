using System.ComponentModel.DataAnnotations;

namespace OutfitShop.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết cho sản phẩm.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá bán.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán không được là số âm.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng cung cấp đường dẫn hình ảnh (ImageUrl).")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục cho sản phẩm.")]
        public int CategoryId { get; set; }
    }
}