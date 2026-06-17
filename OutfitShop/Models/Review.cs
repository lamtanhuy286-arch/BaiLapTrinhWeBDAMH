using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutfitShop.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public int Rating { get; set; }

        // Bổ sung: Trạng thái duyệt bình luận
        public bool IsApproved { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}