using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutfitShop.Interfaces;
using OutfitShop.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IRepository<Review> _reviewRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(
            IProductService productService,
            IRepository<Review> reviewRepo,
            UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _reviewRepo = reviewRepo;
            _userManager = userManager;
        }

        // --- DANH SÁCH SẢN PHẨM (ĐÃ THÊM TÍNH NĂNG SẮP XẾP) ---
        public async Task<IActionResult> Index(string sortOrder)
        {
            var products = await _productService.GetAllProductsAsync();

            // Xử lý sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                default:
                    products = products.OrderByDescending(p => p.Id).ToList(); // Mặc định: Mới nhất
                    break;
            }

            return View(products);
        }

        // --- TÌM KIẾM SẢN PHẨM (ĐÃ THÊM TÍNH NĂNG SẮP XẾP) ---
        public async Task<IActionResult> Search(string query, string sortOrder)
        {
            var products = await _productService.GetAllProductsAsync();

            if (!string.IsNullOrEmpty(query))
            {
                products = products.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Xử lý sắp xếp (kết hợp với kết quả tìm kiếm)
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                default:
                    products = products.OrderByDescending(p => p.Id).ToList(); // Mặc định: Mới nhất
                    break;
            }

            // Dùng chung giao diện với trang Index nhưng dữ liệu đã được lọc và sắp xếp
            return View("Index", products);
        }

        // --- CHI TIẾT SẢN PHẨM ---
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // --- GỬI ĐÁNH GIÁ (AJAX) ---
        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, string comment, int rating)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá!" });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng!" });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Comment = comment,
                Rating = rating
            };

            await _reviewRepo.AddAsync(review);

            return Json(new { success = true, message = "Đánh giá sản phẩm thành công!" });
        }
    }
}