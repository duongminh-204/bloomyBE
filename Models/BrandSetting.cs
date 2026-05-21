using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class BrandSetting
    {
        public int Id { get; set; } = 1; // Singleton pattern

        [Required, MaxLength(200)]
        public string ShopName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;

        public string AboutUs { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}