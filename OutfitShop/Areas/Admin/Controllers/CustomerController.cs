using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutfitShop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CustomerController(ApplicationDbContext context) { _context = context; }

        public class CustomerMock { public string Id { get; set; } = string.Empty; public string FullName { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string PhoneNumber { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            var data = users.Select(u => new CustomerMock
            {
                Id = u.Id,
                FullName = u.FullName ?? "Chưa cập nhật",
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? "Trống",
                Status = (u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow) ? "Bị khóa" : "Hoạt động"
            }).ToList();
            return View(data);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? RedirectToAction("Index") : View(user);
        }

        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Khóa 100 năm hoặc mở khóa
                user.LockoutEnd = (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow) ? null : DateTimeOffset.UtcNow.AddYears(100);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}