using Microsoft.AspNetCore.Mvc;
using OutfitShop.Models;
using OutfitShop.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OutfitShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        // Tiêm ProductService vào để lấy dữ liệu sản phẩm cho trang chủ
        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy tất cả sản phẩm từ Database truyền sang giao diện
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}