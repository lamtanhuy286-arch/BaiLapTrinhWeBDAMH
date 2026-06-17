using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OutfitShop.DTOs;
using OutfitShop.Interfaces;
using OutfitShop.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OutfitShop.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --- XỬ LÝ CHO WEBSITE ---
        public async Task<IdentityResult> RegisterAsync(RegisterDTO model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };
            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<bool> LoginAsync(LoginDTO model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // --- XỬ LÝ CHO API (Theo code gốc của bạn) ---
        public string GenerateJwtToken(ApplicationUser user, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["JWT:Issuer"],
                audience: config["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}