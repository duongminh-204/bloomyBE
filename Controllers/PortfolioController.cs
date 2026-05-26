using Bloomy.Data;
using Bloomy.DTOs.Portfolio;
using Bloomy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/portfolios")]
    [AllowAnonymous]
    public class PortfolioController : ControllerBase
    {
        private readonly BloomyDbContext _context;

        public PortfolioController(BloomyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? eventTypeId = null)
        {
            var query = _context.PortfolioItems
                .AsNoTracking()
                .Include(x => x.EventType)
                .Include(x => x.Images)
                .AsQueryable();

            if (eventTypeId.HasValue)
                query = query.Where(x => x.EventTypeId == eventTypeId.Value);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = items.Select(MapListItem).ToList();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _context.PortfolioItems
                .AsNoTracking()
                .Include(x => x.EventType)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return NotFound(new { message = "Không tìm thấy portfolio." });

            return Ok(MapDetail(item));
        }

        private static PortfolioListItemDto MapListItem(PortfolioItem item)
        {
            var orderedImages = item.Images
                .OrderBy(image => image.OrderIndex)
                .ThenBy(image => image.Id)
                .ToList();

            return new PortfolioListItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                EventTypeId = item.EventTypeId,
                EventTypeName = item.EventType?.Name ?? string.Empty,
                CoverImageUrl = orderedImages.FirstOrDefault()?.ImageUrl ?? string.Empty,
                CreatedAt = item.CreatedAt,
                ImageCount = orderedImages.Count
            };
        }

        private static PortfolioDetailDto MapDetail(PortfolioItem item)
        {
            var orderedImages = item.Images
                .OrderBy(image => image.OrderIndex)
                .ThenBy(image => image.Id)
                .Select(image => new PortfolioImageDto
                {
                    Id = image.Id,
                    ImageUrl = image.ImageUrl,
                    OrderIndex = image.OrderIndex
                })
                .ToList();

            return new PortfolioDetailDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                EventTypeId = item.EventTypeId,
                EventTypeName = item.EventType?.Name ?? string.Empty,
                CreatedAt = item.CreatedAt,
                Images = orderedImages
            };
        }
    }
}