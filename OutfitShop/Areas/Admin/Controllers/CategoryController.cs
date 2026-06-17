using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutfitShop.Data;
using OutfitShop.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context) { _context = context; }

        public class CategoryMock { public int Id { get; set; } public string Name { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public bool IsActive { get; set; } }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Categories
                .Select(c => new CategoryMock
                {
                    Id = c.Id,
                    Name = string.IsNullOrEmpty(c.Name) ? "Chưa có tên" : c.Name,
                    Description = "Danh mục sản phẩm",
                    IsActive = c.IsActive // Đã lấy trạng thái thực tế từ CSDL
                })
                .ToListAsync();
            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Category model)
        {
            // Bỏ qua kiểm tra danh sách sản phẩm bên trong
            ModelState.Remove("Products");

            if (ModelState.IsValid) { _context.Categories.Add(model); await _context.SaveChangesAsync(); return RedirectToAction("Index"); }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category model)
        {
            // Bỏ qua kiểm tra danh sách sản phẩm bên trong
            ModelState.Remove("Products");

            if (ModelState.IsValid) { _context.Categories.Update(model); await _context.SaveChangesAsync(); return RedirectToAction("Index"); }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null) { _context.Categories.Remove(category); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}