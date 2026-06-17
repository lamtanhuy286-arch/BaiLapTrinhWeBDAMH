using Microsoft.AspNetCore.Identity;
using OutfitShop.DTOs;
using System.Threading.Tasks;

namespace OutfitShop.Interfaces
{
    public interface IAuthService
    {
        // Hàm xử lý đăng ký tài khoản
        Task<IdentityResult> RegisterAsync(RegisterDTO model);

        // Hàm xử lý đăng nhập (Trả về true nếu thành công, false nếu sai pass/email)
        Task<bool> LoginAsync(LoginDTO model);

        // Hàm xử lý đăng xuất
        Task LogoutAsync();
    }
}