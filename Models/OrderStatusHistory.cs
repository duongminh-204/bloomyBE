using Bloomy.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class OrderStatusHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatus Status { get; set; }

        public string Notes { get; set; } = string.Empty;

        public Guid UpdatedById { get; set; }
        public User UpdatedBy { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}