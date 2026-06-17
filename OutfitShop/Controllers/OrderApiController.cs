using Microsoft.AspNetCore.Mvc;
using OutfitShop.Interfaces;
using System.Threading.Tasks;

namespace OutfitShop.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderApiController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderApiController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // API lấy lịch sử mua hàng của 1 User cụ thể
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        // API lấy thông tin chi tiết một mã hóa đơn
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy thông tin đơn hàng này" });

            return Ok(order);
        }
    }
}