using Microsoft.AspNetCore.Mvc;
using OutfitShop.DTOs;
using OutfitShop.Interfaces;
using System.Threading.Tasks;

namespace OutfitShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // --- TRANG ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.RegisterAsync(model);
            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // --- TRANG ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var isSuccess = await _authService.LoginAsync(model);
            if (isSuccess)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // --- ĐĂNG XUẤT ---
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}