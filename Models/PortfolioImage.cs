using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class PortfolioImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PortfolioItemId { get; set; }
        public PortfolioItem PortfolioItem { get; set; } = null!;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public int OrderIndex { get; set; }
    }
}