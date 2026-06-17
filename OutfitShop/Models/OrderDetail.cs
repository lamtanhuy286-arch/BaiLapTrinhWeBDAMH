using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutfitShop.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}