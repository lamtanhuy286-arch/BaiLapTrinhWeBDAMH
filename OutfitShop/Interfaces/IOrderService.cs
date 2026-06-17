using OutfitShop.DTOs;
using OutfitShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutfitShop.Interfaces
{
    public interface IOrderService
    {
        // 1. Tạo đơn hàng mới khi khách hàng nhấn Thanh toán
        // Truyền vào ID của User đăng nhập, thông tin form Checkout (OrderDTO) và danh sách giỏ hàng
        Task<Order> CreateOrderAsync(string userId, OrderDTO orderDto, List<CartItem> cart);

        // 2. Lấy danh sách lịch sử đơn hàng của một khách hàng cụ thể
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);

        // 3. Lấy chi tiết một đơn hàng (bao gồm cả các sản phẩm bên trong)
        Task<Order?> GetOrderDetailsAsync(int orderId);

        // 4. Dành cho Admin: Cập nhật trạng thái đơn hàng (Đang xử lý, Đang giao, Đã hoàn thành...)
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    }
}