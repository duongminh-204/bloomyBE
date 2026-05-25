using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bloomy.Models
{
    public class Concept
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string ToneColor { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;

        public Guid? CustomerId { get; set; }
        public User? Customer { get; set; }

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        public bool IsTemplate { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuotedAmount { get; set; }

        public bool IsQuoteApproved { get; set; } = false;

        public string AiGeneratedData { get; set; } = string.Empty; // JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConceptImage> Images { get; set; } = new List<ConceptImage>();
    }
}