using Microsoft.EntityFrameworkCore;
using OutfitShop.Data;
using OutfitShop.DTOs;
using OutfitShop.Interfaces;
using OutfitShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(string userId, OrderDTO orderDto, List<CartItem> cart)
        {
            // Chuyển giỏ hàng thành dữ liệu Order chuẩn
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Đang xử lý", // Trạng thái mặc định
                TotalAmount = cart.Sum(x => x.TotalPrice),
                OrderDetails = cart.Select(c => new OrderDetail
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            // Join các bảng Order -> OrderDetails -> Product để lấy tên sản phẩm
            return await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}