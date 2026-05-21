using Bloomy.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bloomy.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CustomerId { get; set; }    
        public User Customer { get; set; } = null!;

        public Guid? ShopOwnerId { get; set; }
        public User? ShopOwner { get; set; }

        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }

        public Guid? ConceptId { get; set; }
        public Concept? Concept { get; set; }

        [Required, MaxLength(200)]
        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
        public TimeSpan SetupTime { get; set; }

        [Required, MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty; 

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string Notes { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}