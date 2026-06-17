using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OutfitShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}