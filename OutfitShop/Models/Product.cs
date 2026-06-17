using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutfitShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        // Cho phép mô tả dài
        public string Description { get; set; } = "Chưa có mô tả chi tiết";

        [Required]
        [Range(1000, 100000000, ErrorMessage = "Giá sản phẩm phải từ 1.000đ đến 100.000.000đ")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        // Trường mới: Số lượng tồn kho
        [Range(0, 10000, ErrorMessage = "Số lượng không hợp lệ")]
        public int StockQuantity { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}