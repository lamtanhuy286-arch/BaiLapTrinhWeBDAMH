using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutfitShop.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Bổ sung cột này vì View Quản lý đang dùng để bật/tắt danh mục
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Product>? Products { get; set; }
    }
}