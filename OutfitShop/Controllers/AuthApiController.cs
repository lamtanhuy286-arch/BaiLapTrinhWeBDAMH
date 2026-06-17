using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutfitShop.DTOs;
using OutfitShop.Models;
using OutfitShop.Interfaces;
using System.Threading.Tasks;

namespace OutfitShop.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthApiController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _authService = authService;
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gọi trực tiếp hàm xử lý logic đăng ký đã viết ở AuthService
            var result = await _authService.RegisterAsync(model);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }
    }
}