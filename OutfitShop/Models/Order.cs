using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutfitShop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Chờ xác nhận";

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}