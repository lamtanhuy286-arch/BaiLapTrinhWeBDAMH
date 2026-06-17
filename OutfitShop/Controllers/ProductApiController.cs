using Microsoft.AspNetCore.Mvc;
using OutfitShop.Interfaces;
using System.Threading.Tasks;

namespace OutfitShop.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductApiController(IProductService productService)
        {
            _productService = productService;
        }

        // Lấy tất cả sản phẩm trả về JSON
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // Lấy chi tiết 1 sản phẩm bằng ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm yêu cầu" });

            return Ok(product);
        }
    }
}