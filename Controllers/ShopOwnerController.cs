using Bloomy.DTOs;
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
        private readonly IPaymentSettingsService _paymentSettings;

        public ShopOwnerController(IOrderService orderService, IPaymentSettingsService paymentSettings)
        {
            _orderService = orderService;
            _paymentSettings = paymentSettings;
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

        [HttpGet("bookings/managed")]
        public async Task<IActionResult> ManagedBookings()
        {
            var list = await _orderService.GetManagedBookingsAsync();
            return Ok(list);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> Calendar([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var start = (from ?? DateTime.UtcNow.Date).Date;
            var end = (to ?? start.AddDays(42)).Date;
            try
            {
                var events = await _orderService.GetCalendarEventsAsync(start, end);
                return Ok(events);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpPatch("booking/{id:guid}/internal-notes")]
        public async Task<IActionResult> UpdateInternalNotes(Guid id, [FromBody] UpdateInternalNotesDto dto)
        {
            try
            {
                var result = await _orderService.UpdateInternalNotesAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("booking/{id:guid}/schedule")]
        public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] ShopOwnerRescheduleDto dto)
        {
            try
            {
                var result = await _orderService.ShopOwnerRescheduleAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("booking/{id:guid}/reschedule-request/resolve")]
        public async Task<IActionResult> ResolveRescheduleRequest(Guid id, [FromBody] HandleCustomerRequestDto dto)
        {
            try
            {
                var result = await _orderService.ResolveRescheduleRequestAsync(id, GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("booking/{id:guid}/cancel-request/resolve")]
        public async Task<IActionResult> ResolveCancelRequest(Guid id, [FromBody] HandleCustomerRequestDto dto)
        {
            try
            {
                var result = await _orderService.ResolveCancelRequestAsync(id, GetUserId(), dto);
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

        [HttpGet("payment-settings")]
        public async Task<IActionResult> GetPaymentSettings()
        {
            return Ok(await _paymentSettings.GetAsync());
        }

        [HttpPut("payment-settings")]
        public async Task<IActionResult> UpdatePaymentSettings([FromBody] UpdatePaymentSettingsDto dto)
        {
            try
            {
                var result = await _paymentSettings.UpdateAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("payment-settings/qr-upload")]
        public async Task<IActionResult> UploadQrImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file ảnh QR." });

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _paymentSettings.SaveQrImageAsync(stream, file.FileName);
                var current = await _paymentSettings.GetAsync();
                var updated = await _paymentSettings.UpdateAsync(new UpdatePaymentSettingsDto
                {
                    QrImageUrl = url,
                    AccountHolderName = current.AccountHolderName,
                    AccountNumber = current.AccountNumber,
                    BankName = current.BankName,
                    TransferContentTemplate = current.TransferContentTemplate,
                    MomoPhone = current.MomoPhone,
                    EnableMomo = current.EnableMomo,
                    EnableQrCode = current.EnableQrCode,
                    EnableBankTransfer = current.EnableBankTransfer,
                    EnableVNPay = current.EnableVNPay
                });
                return Ok(new { qrImageUrl = url, settings = updated });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("payments/pending")]
        public async Task<IActionResult> PendingPayments()
        {
            var list = await _orderService.GetPendingPaymentConfirmationsAsync();
            return Ok(list);
        }

        [HttpPost("booking/{orderId:guid}/payments/{paymentId:guid}/confirm")]
        public async Task<IActionResult> ConfirmPayment(Guid orderId, Guid paymentId)
        {
            try
            {
                var result = await _orderService.ConfirmPaymentAsync(orderId, paymentId, GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
