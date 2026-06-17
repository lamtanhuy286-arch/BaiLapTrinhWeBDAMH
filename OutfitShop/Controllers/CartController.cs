using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutfitShop.Models;
using OutfitShop.Helpers;
using OutfitShop.Interfaces;
using OutfitShop.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutfitShop.Controllers
{
    public class CartController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        // Bổ sung IProductService để lấy thông tin sản phẩm khi AddToCart
        private readonly IProductService _productService;

        public CartController(
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            IProductService productService)
        {
            _orderService = orderService;
            _userManager = userManager;
            _productService = productService;
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        public IActionResult Index()
        {
            return View(GetCart());
        }

        // ==========================================
        // 1. HÀM THÊM VÀO GIỎ HÀNG 
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            // Kiểm tra xem sản phẩm có tồn tại trong DB không
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                // Nếu đã có trong giỏ -> Tăng số lượng
                item.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có -> Thêm mới
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            // Lưu lại vào Session
            HttpContext.Session.SetObject("Cart", cart);
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng thành công!" });
        }

        // ==========================================
        // 2. HÀM CẬP NHẬT SỐ LƯỢNG 
        // ==========================================
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                item.Quantity = quantity;
                HttpContext.Session.SetObject("Cart", cart);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // ==========================================
        // 3. HÀM XÓA KHỎI GIỎ HÀNG 
        // ==========================================
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject("Cart", cart);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ." });
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ THANH TOÁN (ĐÃ NÂNG CẤP MOMO)
        // ==========================================
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index");
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(OrderDTO orderDto, string PaymentMethod)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index");

            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index");

            if (PaymentMethod == "COD")
            {
                var order = await _orderService.CreateOrderAsync(userId, orderDto, cart);
                HttpContext.Session.Remove("Cart");
                return RedirectToAction("OrderSuccess");
            }
            else if (PaymentMethod == "Momo")
            {
                // LƯU TẠM THÔNG TIN GIAO HÀNG VÀO SESSION TRƯỚC KHI SANG MOMO
                HttpContext.Session.SetObject("PendingOrder", orderDto);
                return RedirectToAction("CreatePayment", "Order");
            }

            return View("Checkout", cart);
        }

        [HttpGet]
        public async Task<IActionResult> OrderSuccess([FromQuery] int? resultCode)
        {
            // NẾU LÀ MOMO TRẢ VỀ CÓ RESULT CODE
            if (resultCode.HasValue)
            {
                if (resultCode.Value == 0) // resultCode = 0: Thanh toán thành công
                {
                    // Lấy lại thông tin khách hàng nhập lúc nãy
                    var pendingOrder = HttpContext.Session.GetObject<OrderDTO>("PendingOrder");
                    var cart = GetCart();
                    var userId = _userManager.GetUserId(User);

                    if (pendingOrder != null && cart.Count > 0 && !string.IsNullOrEmpty(userId))
                    {
                        // Lưu đơn hàng vào SQL Server
                        await _orderService.CreateOrderAsync(userId, pendingOrder, cart);

                        // Xóa Session
                        HttpContext.Session.Remove("Cart");
                        HttpContext.Session.Remove("PendingOrder");
                    }
                }
                else
                {
                    // resultCode != 0: Bị hủy hoặc lỗi
                    TempData["Error"] = "Giao dịch MoMo đã bị hủy hoặc thanh toán thất bại!";
                    return RedirectToAction("Checkout");
                }
            }

            // Trả về view thành công (Cho cả COD và Momo thành công)
            return View();
        }
    }
}