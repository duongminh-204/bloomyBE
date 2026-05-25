using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return Ok(new { message = "Admin dashboard" });
        }

        [HttpGet("users")]
        public IActionResult ManageUsers()
        {
            return Ok(new { message = "List users (stub)" });
        }

        [HttpGet("roles")]
        public IActionResult ManageRoles()
        {
            return Ok(new { message = "List roles (stub)" });
        }

        [HttpGet("bookings")]
        public IActionResult AllBookings()
        {
            return Ok(new { message = "All bookings (admin)" });
        }

        [HttpGet("transactions")]
        public IActionResult Transactions()
        {
            return Ok(new { message = "All transactions (admin)" });
        }

        [HttpGet("settings")]
        public IActionResult Settings()
        {
            return Ok(new { message = "System settings (stub)" });
        }
    }
}
