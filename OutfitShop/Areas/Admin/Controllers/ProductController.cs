using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OutfitShop.Data;
using OutfitShop.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public class ProductMock { public int Id { get; set; } public string Name { get; set; } = string.Empty; public string Category { get; set; } = string.Empty; public double Price { get; set; } public bool IsActive { get; set; } public string ImageUrl { get; set; } = string.Empty; }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Products.Include(p => p.Category)
                .Select(p => new ProductMock
                {
                    Id = p.Id,
                    Name = string.IsNullOrEmpty(p.Name) ? "Chưa có tên" : p.Name,
                    Category = p.Category != null ? p.Category.Name : "Chưa phân loại",
                    Price = (double)p.Price,
                    IsActive = p.IsActive,
                    ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? "/images/no-image.png" : p.ImageUrl
                })
                .ToListAsync();
            return View(data);
        }

        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile? ImageFile)
        {
            // Kiểm tra xem danh mục có tồn tại không
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Danh mục không hợp lệ. Vui lòng chọn lại.");
            }

            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    model.ImageUrl = "/images/products/" + uniqueFileName;
                }
                else
                {
                    model.ImageUrl = "/images/no-image.png";
                }

                _context.Products.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile? ImageFile)
        {
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(model.Id);
                if (existingProduct == null) return NotFound();

                existingProduct.Name = model.Name;
                existingProduct.Price = model.Price;
                existingProduct.CategoryId = model.CategoryId;
                existingProduct.IsActive = model.IsActive;
                existingProduct.Description = model.Description;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    existingProduct.ImageUrl = "/images/products/" + uniqueFileName;
                }

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) { _context.Products.Remove(product); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}