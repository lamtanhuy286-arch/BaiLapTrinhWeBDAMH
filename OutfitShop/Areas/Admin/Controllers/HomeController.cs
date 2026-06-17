using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutfitShop.Data;   // Đảm bảo có ApplicationDbContext
using OutfitShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu thật từ Database
            var totalRev = await _context.Orders.Where(o => o.Status != "Đã hủy").SumAsync(o => o.TotalAmount);

            ViewBag.TotalRevenue = totalRev.ToString("N0") + " VND";
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalCustomers = await _context.Users.CountAsync();
            ViewBag.TotalReviews = await _context.Reviews.CountAsync();

            return View();
        }
    }
}