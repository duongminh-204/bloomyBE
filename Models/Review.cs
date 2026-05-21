using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class Review
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public Guid CustomerId { get; set; }
        public User Customer { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = false; // Chủ tiệm duyệt để hiển thị

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}