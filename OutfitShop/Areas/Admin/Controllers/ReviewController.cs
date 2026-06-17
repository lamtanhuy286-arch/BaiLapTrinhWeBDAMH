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
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReviewController(ApplicationDbContext context) { _context = context; }

        public class ReviewMock { public int Id { get; set; } public string CustomerName { get; set; } = string.Empty; public string ProductName { get; set; } = string.Empty; public int Rating { get; set; } public string Comment { get; set; } = string.Empty; public bool IsApproved { get; set; } = true; }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews.Include(r => r.Product).Include(r => r.User).ToListAsync();
            var data = reviews.Select(r => new ReviewMock
            {
                Id = r.Id,
                CustomerName = r.User?.FullName ?? "Ẩn danh",
                ProductName = r.Product?.Name ?? "Sản phẩm",
                Rating = r.Rating,
                Comment = r.Comment,
                IsApproved = true
            }).ToList();

            return View(data);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null) { _context.Reviews.Remove(review); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}