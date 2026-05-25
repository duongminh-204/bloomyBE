using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        [HttpPost("save-concept")]
        public IActionResult SaveConcept()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "Concept saved", userId });
        }

        [HttpGet("saved-concepts")]
        public IActionResult GetSavedConcepts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "Saved concepts for user", userId });
        }

        [HttpPost("bookings")]
        public IActionResult CreateBooking()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "Booking created", userId });
        }

        [HttpGet("bookings/my")]
        public IActionResult MyBookings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "My bookings", userId });
        }

        [HttpPost("payments")]
        public IActionResult Payment()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "Payment processed (stub)", userId });
        }

        [HttpGet("tracking/{id}")]
        public IActionResult Track(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "Tracking info", id, userId });
        }
    }
}
