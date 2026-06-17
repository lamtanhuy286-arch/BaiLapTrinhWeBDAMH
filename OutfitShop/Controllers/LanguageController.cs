using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace OutfitShop.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult Change(string culture, string returnUrl)
        {
            // 1. Tạo Cookie để lưu lại lựa chọn ngôn ngữ của người dùng trên trình duyệt (Hết hạn sau 1 năm)
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // 2. Chuyển hướng người dùng quay trở lại đúng trang họ đang xem trước khi bấm đổi ngôn ngữ
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Nếu không tìm thấy trang cũ (hoặc trang ngoài hệ thống), quay về trang chủ
            return RedirectToAction("Index", "Home");
        }
    }
}