namespace Bloomy.DTOs.Portfolio
{
    public class PortfolioImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    public class PortfolioListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? EventTypeId { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }
    }

    public class PortfolioDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? EventTypeId { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<PortfolioImageDto> Images { get; set; } = new();
    }
}