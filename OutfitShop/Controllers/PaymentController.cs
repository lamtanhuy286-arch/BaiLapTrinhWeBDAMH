using Microsoft.AspNetCore.Mvc;
using OutfitShop.Helpers;
using OutfitShop.Models;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;

namespace OutfitShop.Controllers
{
    public class PaymentController : Controller
    {
        // 1. Tiếp nhận hành động bấm nút Thanh toán Visa từ Giỏ hàng
        [HttpPost]
        public IActionResult CreateCheckoutSession()
        {
            // Lấy dữ liệu giỏ hàng hiện tại ra từ Session
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart");

            // Nếu giỏ hàng trống, đẩy ngược về trang giỏ hàng
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Lấy tên miền động của website
            var domain = $"{Request.Scheme}://{Request.Host}";

            // Thiết lập cấu hình gửi lên server Stripe
            var options = new SessionCreateOptions
            {
                // Chỉ định hình thức quẹt thẻ (Chấp nhận Visa, Mastercard, JCB,...)
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                // Đường dẫn đón khách về khi họ thanh toán THÀNH CÔNG
                SuccessUrl = domain + "/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
                // Đường dẫn đón khách về nếu họ bấm nút HỦY
                CancelUrl = domain + "/Payment/Cancel",
            };

            // Duyệt toàn bộ sản phẩm trong Giỏ hàng chuyển sang định dạng hóa đơn của Stripe
            foreach (var item in cart)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price,
                        Currency = "vnd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions // SỬA LỖI: Đổi tên Class cho đúng SDK mới
                        {
                            Name = item.ProductName,
                        },
                    },
                    Quantity = item.Quantity,
                };
                options.LineItems.Add(sessionLineItem);
            }

            // Gọi SDK Stripe tạo phiên (Session) giao dịch bảo mật
            var service = new SessionService();
            Session session = service.Create(options);

            // SỬA LỖI: Thay thế .Add bằng cách gán indexer hoặc .Append để tránh lỗi trùng lặp Key Header
            Response.Headers["Location"] = session.Url;
            return new StatusCodeResult(303);
        }

        // 2. Điểm đón nhận kết quả trả về khi giao dịch THÀNH CÔNG
        [HttpGet]
        public IActionResult Success(string session_id)
        {
            var service = new SessionService();
            Session session = service.Get(session_id);

            // Kiểm tra trạng thái hóa đơn trên Stripe
            if (session.PaymentStatus == "paid")
            {
                // TODO: Thực hiện các hàm lưu đơn hàng vào Cơ sở dữ liệu tại đây

                // Dọn sạch giỏ hàng trong Session sau khi thanh toán xong
                HttpContext.Session.Remove("Cart");

                ViewBag.Message = "Hệ thống đã ghi nhận khoản thanh toán từ thẻ Visa của bạn.";
                return View();
            }

            return RedirectToAction("Cancel");
        }

        // 3. Điểm đón nhận kết quả khi giao dịch THẤT BẠI hoặc HỦY BỎ
        [HttpGet]
        public IActionResult Cancel()
        {
            ViewBag.Message = "Giao dịch thanh toán đã bị đóng hoặc phát sinh lỗi trong quá trình xử lý thẻ.";
            return View();
        }
    }
}