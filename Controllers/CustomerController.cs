using Bloomy.DTOs.Orders;
using Bloomy.Models;
using BloomyBE.Services.Interfaces;
using Bloomy.Data.Interfaces;
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
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepo;

        public CustomerController(IOrderService orderService, IOrderRepository orderRepo)
        {
            _orderService = orderService;
            _orderRepo = orderRepo;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var result = await _orderService.CreateBookingAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("bookings/{id:guid}")]
        public async Task<IActionResult> GetBooking(Guid id)
        {
            var result = await _orderService.GetBookingAsync(id, GetUserId());
            if (result == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });
            return Ok(result);
        }

        [HttpGet("bookings/my")]
        public async Task<IActionResult> MyBookings()
        {
            var list = await _orderService.GetMyBookingsAsync(GetUserId());
            return Ok(list);
        }

        [HttpGet("tracking/{id:guid}")]
        public async Task<IActionResult> Track(Guid id)
        {
            try
            {
                var result = await _orderService.TrackBookingAsync(id, GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("bookings/{id:guid}/payments")]
        public async Task<IActionResult> CreatePayment(Guid id, [FromBody] CreatePaymentDto dto)
        {
            try
            {
                var result = await _orderService.CreatePaymentAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bookings/{id:guid}/reschedule")]
        public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleBookingDto dto)
        {
            try
            {
                var result = await _orderService.RescheduleBookingAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bookings/{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingDto dto)
        {
            try
            {
                var result = await _orderService.CancelBookingAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bookings/{id:guid}/review")]
        public async Task<IActionResult> SubmitReview(Guid id, [FromBody] SubmitReviewDto dto)
        {
            try
            {
                var result = await _orderService.SubmitReviewAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("concepts/{id:guid}/approve-quote")]
        public async Task<IActionResult> ApproveConceptQuote(Guid id, [FromBody] ApproveQuoteDto? dto)
        {
            try
            {
                var result = await _orderService.ApproveConceptQuoteAsync(id, GetUserId(), dto?.QuotedAmount);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("save-concept")]
        public async Task<IActionResult> SaveConcept([FromBody] SaveConceptDto dto)
        {
            var concept = new Concept
            {
                Name = dto.Name ?? "Concept Bloomy",
                Description = dto.Description ?? "",
                ToneColor = dto.ToneColor ?? "",
                Style = dto.Style ?? "",
                CustomerId = GetUserId(),
                QuotedAmount = dto.QuotedAmount ?? 0,
                IsQuoteApproved = false,
                CreatedAt = DateTime.UtcNow
            };
            var saved = await _orderRepo.CreateConceptAsync(concept);
            return Ok(new { id = saved.Id, saved.Name, saved.QuotedAmount });
        }

        [HttpGet("saved-concepts")]
        public async Task<IActionResult> GetSavedConcepts()
        {
            var list = await _orderRepo.GetConceptsByCustomerAsync(GetUserId());
            return Ok(list.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.QuotedAmount,
                c.IsQuoteApproved,
                c.CreatedAt
            }));
        }
    }

    public class SaveConceptDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ToneColor { get; set; }
        public string? Style { get; set; }
        public decimal? QuotedAmount { get; set; }
    }

    public class ApproveQuoteDto
    {
        public decimal? QuotedAmount { get; set; }
    }
}
