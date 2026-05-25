using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ShopOwner")]
    public class ShopOwnerController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "ShopOwner dashboard", userId });
        }

        [HttpGet("requests")]
        public IActionResult ViewRequests()
        {
            return Ok(new { message = "List of booking requests" });
        }

        [HttpPut("booking/{id}/status")]
        public IActionResult UpdateBookingStatus(string id)
        {
            return Ok(new { message = "Booking status updated", id });
        }

        [HttpPost("services")]
        public IActionResult CreateService()
        {
            return Ok(new { message = "Service created (stub)" });
        }

        [HttpGet("chat/{customerId}")]
        public IActionResult ChatWithCustomer(string customerId)
        {
            return Ok(new { message = "Chat endpoint (stub)", customerId });
        }
    }
}
