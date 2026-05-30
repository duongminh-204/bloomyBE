using Bloomy.DTOs;
using Bloomy.DTOs.Portfolio;
using Bloomy.DTOs.Orders;
using Bloomy.Data;
using Bloomy.Models;
using BloomyBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BloomyBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ShopOwner")]
    public class ShopOwnerController : ControllerBase
    {
        private static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private readonly IOrderService _orderService;
        private readonly IPaymentSettingsService _paymentSettings;
        private readonly BloomyDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ICurrentShopContext _shopContext;

        public ShopOwnerController(
            IOrderService orderService,
            IPaymentSettingsService paymentSettings,
            BloomyDbContext context,
            IWebHostEnvironment env,
            ICurrentShopContext shopContext)
        {
            _orderService = orderService;
            _paymentSettings = paymentSettings;
            _context = context;
            _env = env;
            _shopContext = shopContext;
        }

        private Guid GetUserId() => _shopContext.UserId;
        private Guid GetShopId() => _shopContext.RequireShopId();

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var data = await _orderService.GetShopOwnerDashboardAsync(GetShopId());
            return Ok(data);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> ViewRequests()
        {
            var list = await _orderService.GetPendingBookingsAsync(GetShopId());
            return Ok(list);
        }

        [HttpGet("bookings/upcoming")]
        public async Task<IActionResult> UpcomingSetups()
        {
            var list = await _orderService.GetUpcomingSetupsAsync(GetShopId());
            return Ok(list);
        }

        [HttpGet("bookings/managed")]
        public async Task<IActionResult> ManagedBookings()
        {
            var list = await _orderService.GetManagedBookingsAsync(GetShopId());
            return Ok(list);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> Calendar([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var start = (from ?? DateTime.UtcNow.Date).Date;
            var end = (to ?? start.AddDays(42)).Date;
            try
            {
                var events = await _orderService.GetCalendarEventsAsync(GetShopId(), start, end);
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
                var result = await _orderService.GetBookingForShopAsync(id, GetShopId());
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
                var result = await _orderService.ConfirmBookingAsync(id, GetShopId(), GetUserId(), dto);
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
                var result = await _orderService.UpdateBookingStatusAsync(id, GetShopId(), GetUserId(), dto);
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
                var result = await _orderService.UpdateInternalNotesAsync(id, GetShopId(), GetUserId(), dto);
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
                var result = await _orderService.ShopOwnerRescheduleAsync(id, GetShopId(), GetUserId(), dto);
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
                var result = await _orderService.ResolveRescheduleRequestAsync(id, GetShopId(), GetUserId(), dto);
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
                var result = await _orderService.ResolveCancelRequestAsync(id, GetShopId(), GetUserId(), dto);
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

        [HttpGet("portfolios")]
        public async Task<IActionResult> GetPortfolios([FromQuery] int? eventTypeId = null)
        {
            var shopId = GetShopId();
            var query = _context.PortfolioItems
                .AsNoTracking()
                .Where(x => x.ShopId == shopId)
                .Include(x => x.EventType)
                .Include(x => x.Images)
                .AsQueryable();

            if (eventTypeId.HasValue)
                query = query.Where(x => x.EventTypeId == eventTypeId.Value);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(items.Select(MapListItem).ToList());
        }

        [HttpGet("portfolios/{id:guid}")]
        public async Task<IActionResult> GetPortfolio(Guid id)
        {
            var item = await LoadPortfolioAsync(id);
            if (item == null)
                return NotFound(new { message = "Không tìm thấy portfolio." });

            return Ok(MapDetail(item));
        }

        [HttpPost("portfolios")]
        public async Task<IActionResult> CreatePortfolio([FromForm] UpsertPortfolioDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Vui lòng nhập tiêu đề portfolio." });

            var item = new PortfolioItem
            {
                Id = Guid.NewGuid(),
                ShopId = GetShopId(),
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                EventTypeId = dto.EventTypeId,
                Price = dto.Price ?? 0m,
                ToneColor = dto.ToneColor?.Trim() ?? string.Empty,
                Style = dto.Style?.Trim() ?? string.Empty,
                Tags = dto.Tags?.Trim() ?? string.Empty,
                IndoorOutdoor = dto.IndoorOutdoor?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.PortfolioItems.Add(item);

            try
            {
                await SavePortfolioImagesAsync(item, dto.Images);
                await _context.SaveChangesAsync();
                return Ok(MapDetail(await LoadPortfolioAsync(item.Id) ?? item));
            }
            catch (InvalidOperationException ex)
            {
                _context.PortfolioItems.Remove(item);
                await _context.SaveChangesAsync();
                return BadRequest(new { message = ex.Message });
            }
            catch
            {
                _context.PortfolioItems.Remove(item);
                await _context.SaveChangesAsync();
                throw;
            }
        }

        [HttpPut("portfolios/{id:guid}")]
        public async Task<IActionResult> UpdatePortfolio(Guid id, [FromForm] UpsertPortfolioDto dto)
        {
            var item = await _context.PortfolioItems
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id && x.ShopId == GetShopId());

            if (item == null)
                return NotFound(new { message = "Không tìm thấy portfolio." });

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Vui lòng nhập tiêu đề portfolio." });

            item.Title = dto.Title.Trim();
            item.Description = dto.Description?.Trim() ?? string.Empty;
            item.EventTypeId = dto.EventTypeId;
            item.Price = dto.Price ?? 0m;
            item.ToneColor = dto.ToneColor?.Trim() ?? item.ToneColor;
            item.Style = dto.Style?.Trim() ?? item.Style;
            item.Tags = dto.Tags?.Trim() ?? item.Tags;
            item.IndoorOutdoor = dto.IndoorOutdoor?.Trim() ?? item.IndoorOutdoor;

            var shouldReplaceImages = dto.Images?.Count > 0;
            if (shouldReplaceImages)
            {
                foreach (var image in item.Images.ToList())
                {
                    DeleteFileIfExists(image.ImageUrl);
                    _context.PortfolioImages.Remove(image);
                }

                await SavePortfolioImagesAsync(item, dto.Images);
            }

            await _context.SaveChangesAsync();
            return Ok(MapDetail(await LoadPortfolioAsync(item.Id) ?? item));
        }

        [HttpDelete("portfolios/{id:guid}")]
        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            var item = await _context.PortfolioItems
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id && x.ShopId == GetShopId());

            if (item == null)
                return NotFound(new { message = "Không tìm thấy portfolio." });

            foreach (var image in item.Images)
                DeleteFileIfExists(image.ImageUrl);

            _context.PortfolioItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa portfolio." });
        }

        private async Task<PortfolioItem?> LoadPortfolioAsync(Guid id)
        {
            return await _context.PortfolioItems
                .AsNoTracking()
                .Include(x => x.EventType)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id && x.ShopId == GetShopId());
        }

        private async Task SavePortfolioImagesAsync(PortfolioItem item, List<IFormFile>? images)
        {
            if (images == null || images.Count == 0)
                return;

            var uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "portfolios", item.Id.ToString("N"));
            Directory.CreateDirectory(uploadRoot);

            var orderIndex = 0;
            foreach (var image in images.Where(x => x != null && x.Length > 0))
            {
                var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                    throw new InvalidOperationException("Chỉ hỗ trợ ảnh JPG, PNG, WEBP hoặc GIF.");

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadRoot, fileName);

                await using (var fs = System.IO.File.Create(fullPath))
                {
                    await image.CopyToAsync(fs);
                }

                _context.PortfolioImages.Add(new PortfolioImage
                {
                    PortfolioItemId = item.Id,
                    ImageUrl = $"/uploads/portfolios/{item.Id:N}/{fileName}",
                    OrderIndex = orderIndex++
                });
            }
        }

        private void DeleteFileIfExists(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            var normalized = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), normalized);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        private static PortfolioListItemDto MapListItem(PortfolioItem item)
        {
            var orderedImages = item.Images.OrderBy(x => x.OrderIndex).ThenBy(x => x.Id).ToList();

            return new PortfolioListItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                EventTypeId = item.EventTypeId,
                EventTypeName = item.EventType?.Name ?? string.Empty,
                Price = item.Price,
                ToneColor = item.ToneColor,
                Style = item.Style,
                Tags = item.Tags,
                IndoorOutdoor = item.IndoorOutdoor,
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
                Price = item.Price,
                ToneColor = item.ToneColor,
                Style = item.Style,
                Tags = item.Tags,
                IndoorOutdoor = item.IndoorOutdoor,
                CreatedAt = item.CreatedAt,
                Images = orderedImages
            };
        }

        [HttpGet("payments/pending")]
        public async Task<IActionResult> PendingPayments()
        {
            var list = await _orderService.GetPendingPaymentConfirmationsAsync(GetShopId());
            return Ok(list);
        }

        [HttpPost("booking/{orderId:guid}/payments/{paymentId:guid}/confirm")]
        public async Task<IActionResult> ConfirmPayment(Guid orderId, Guid paymentId)
        {
            try
            {
                var result = await _orderService.ConfirmPaymentAsync(orderId, paymentId, GetShopId(), GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
