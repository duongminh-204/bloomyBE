using Bloomy.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/event-types")]
    [AllowAnonymous]
    public class EventTypesController : ControllerBase
    {
        private readonly BloomyDbContext _context;

        public EventTypesController(BloomyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await DatabaseSeeder.SeedAsync(_context);

            var types = await _context.EventTypes
                .OrderBy(e => e.Id)
                .Select(e => new { e.Id, e.Name, e.Description, e.IconUrl })
                .ToListAsync();

            return Ok(types);
        }
    }
}
