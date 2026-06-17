using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OutfitShop.Models;
using OutfitShop.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Net.Http;
using OutfitShop.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace OutfitShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly MomoOption _momoConfig;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            IOptions<MomoOption> momoConfig,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _momoConfig = momoConfig.Value;
            _orderService = orderService;
            _userManager = userManager;
        }

        // --- HÀM XỬ LÝ THANH TOÁN MOMO ---
        public async Task<IActionResult> CreatePayment()
        {
            // 1. Cấu hình API MoMo (Lấy chính xác từ mã Test của bạn)
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = "MOMO1UD420260613_TEST";
            string accessKey = "X2QEH5HnnAJ0S9NO";
            string secretKey = "ayCpmcBNUOE8QHSi4no9DDg8tAHWym0q";

            // 2. Lấy giỏ hàng từ Session để tính tổng tiền
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            long amount = (long)cart.Sum(x => x.Price * x.Quantity);
            string orderInfo = "Thanh toán đơn hàng OUTFITSHOP";
            string orderId = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // 3. Đường dẫn trả về sau khi quét mã thành công (Localhost port 7015)
            string redirectUrl = "https://localhost:7015/Cart/OrderSuccess";
            string ipnUrl = "https://localhost:7015/api/order/momo-ipn";
            string requestType = "captureWallet";

            // 4. Tạo chuỗi dữ liệu gốc (Raw Hash) theo đúng chuẩn thuật toán của MoMo
            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

            // 5. Mã hóa chuỗi bằng thuật toán HMAC SHA256 với Secret Key
            string signature = ComputeHmacSha256(rawHash, secretKey);

            // 6. Đóng gói dữ liệu JSON để gửi đi
            var message = new
            {
                partnerCode = partnerCode,
                partnerName = "OUTFITSHOP",
                storeId = "MomoTestStore",
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = redirectUrl,
                ipnUrl = ipnUrl,
                lang = "vi",
                extraData = extraData,
                requestType = requestType,
                signature = signature
            };

            // 7. Gửi Request sang máy chủ MoMo và nhận link thanh toán
            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("payUrl", out JsonElement payUrlElement))
                    {
                        string payUrl = payUrlElement.GetString();
                        return Redirect(payUrl); // Chuyển hướng người dùng sang trang quét mã QR MoMo
                    }
                    else
                    {
                        return Content("Lỗi từ MoMo: " + responseString);
                    }
                }
            }
        }

        // --- HÀM HỖ TRỢ MÃ HÓA BẢO MẬT HMAC SHA256 ---
        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                var hashmessage = hmacsha256.ComputeHash(messageBytes);
                // MoMo yêu cầu mã Hash phải viết thường
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }

        // Xem toàn bộ lịch sử mua hàng của tài khoản đang đăng nhập
        public async Task<IActionResult> History()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        // Xem chi tiết một đơn hàng cụ thể kèm tên sản phẩm
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null) return NotFound();

            return View(order);
        }
    }
}