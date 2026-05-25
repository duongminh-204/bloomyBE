using Bloomy.DTOs.Orders;
using BloomyBE.Services.Interfaces;
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
        private readonly IOrderService _orderService;

        public ShopOwnerController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var data = await _orderService.GetShopOwnerDashboardAsync();
            return Ok(data);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> ViewRequests()
        {
            var list = await _orderService.GetPendingBookingsAsync();
            return Ok(list);
        }

        [HttpGet("bookings/upcoming")]
        public async Task<IActionResult> UpcomingSetups()
        {
            var list = await _orderService.GetUpcomingSetupsAsync();
            return Ok(list);
        }

        [HttpGet("bookings/{id:guid}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            try
            {
                var result = await _orderService.GetBookingForShopOwnerAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("booking/{id:guid}/confirm")]
        public async Task<IActionResult> ConfirmBooking(Guid id, [FromBody] ConfirmBookingDto dto)
        {
            try
            {
                var result = await _orderService.ConfirmBookingAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("booking/{id:guid}/status")]
        public async Task<IActionResult> UpdateBookingStatus(Guid id, [FromBody] UpdateBookingStatusDto dto)
        {
            try
            {
                var result = await _orderService.UpdateBookingStatusAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
