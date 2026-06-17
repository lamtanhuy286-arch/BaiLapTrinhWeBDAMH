using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutfitShop.Models;

namespace OutfitShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // BẮT BUỘC PHẢI CÓ CÁC DÒNG NÀY THÌ EF CORE MỚI TẠO BẢNG
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Ép kiểu Decimal chuẩn cho SQL Server để không bị cảnh báo
            builder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Entity<OrderDetail>().Property(od => od.Price).HasColumnType("decimal(18,2)");

            // 2. Chống lỗi "Multiple Cascade Paths" (Lỗi kinh điển khiến Update-Database bị sập)
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}