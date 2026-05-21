using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class ConceptImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConceptId { get; set; }
        public Concept Concept { get; set; } = null!;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}