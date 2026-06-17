using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutfitShop.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context) { _context = context; }

        public class OrderMock { public string OrderId { get; set; } = string.Empty; public string CustomerName { get; set; } = string.Empty; public string OrderDate { get; set; } = string.Empty; public double TotalAmount { get; set; } public string Status { get; set; } = string.Empty; }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).ToListAsync();
            var data = orders.Select(o => new OrderMock
            {
                OrderId = o.Id.ToString(),
                CustomerName = o.User?.FullName ?? "Khách vãng lai",
                OrderDate = o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                TotalAmount = (double)o.TotalAmount,
                Status = o.Status ?? "Chờ xác nhận"
            }).ToList();

            return View(data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                // Thêm dấu ! để bỏ qua cảnh báo Nullable của Entity Framework khi gọi ThenInclude
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? RedirectToAction("Index") : View(order);
        }
    }
}