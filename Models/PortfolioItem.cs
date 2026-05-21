using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class PortfolioItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }

        public Guid? OrderId { get; set; } // Liên kết với đơn đã hoàn thành
        public Order? Order { get; set; }

        public ICollection<PortfolioImage> Images { get; set; } = new List<PortfolioImage>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}