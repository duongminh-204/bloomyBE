using System.Text.Json;
using Bloomy.Data.Interfaces;
using Bloomy.DTOs.Orders;
using Bloomy.Models;
using Bloomy.Models.Enums;
using BloomyBE.Configuration;
using BloomyBE.Helpers;
using BloomyBE.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BloomyBE.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly BookingSettings _settings;

        private static readonly Dictionary<OrderStatus, string> StatusLabels = new()
        {
            [OrderStatus.PendingConfirmation] = "Chờ xác nhận",
            [OrderStatus.WaitingDeposit] = "Chờ đặt cọc",
            [OrderStatus.Confirmed] = "Đã xác nhận",
            [OrderStatus.Preparing] = "Đang chuẩn bị",
            [OrderStatus.Transporting] = "Đang vận chuyển",
            [OrderStatus.SettingUp] = "Đang thi công",
            [OrderStatus.Completed] = "Hoàn thành",
            [OrderStatus.Cancelled] = "Đã hủy",
            [OrderStatus.Rejected] = "Bị từ chối"
        };

        public OrderService(IOrderRepository orderRepo, IOptions<BookingSettings> settings)
        {
            _orderRepo = orderRepo;
            _settings = settings.Value;
        }

        public async Task<OrderDto> CreateBookingAsync(Guid customerId, CreateBookingDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.Email))
                throw new InvalidOperationException("Vui lòng nhập đầy đủ họ tên, số điện thoại và email.");

            if (!dto.ConceptId.HasValue)
                throw new InvalidOperationException("Bạn cần chọn và xác nhận concept trước khi đặt lịch.");

            var concept = await _orderRepo.GetConceptAsync(dto.ConceptId.Value)
                ?? throw new InvalidOperationException("Concept không tồn tại.");

            if (concept.CustomerId != customerId && concept.CustomerId != null)
                throw new InvalidOperationException("Concept không thuộc về bạn.");

            if (!concept.IsQuoteApproved)
                throw new InvalidOperationException("Bạn cần xác nhận báo giá concept trước khi đặt lịch.");

            if (!BookingValidator.IsValidServiceArea(dto.Address, dto.District, _settings, out var areaError))
                throw new InvalidOperationException(areaError);

            var eventDate = dto.EventDate.Date;
            if (!BookingValidator.IsFutureOrToday(eventDate, out var dateError))
                throw new InvalidOperationException(dateError);

            var setupTime = ParseSetupTime(dto.SetupTime);

            var availability = await CheckAvailabilityAsync(eventDate, setupTime, null);
            if (!availability.IsAvailable)
            {
                var msg = availability.Message;
                if (availability.SuggestedDates?.Count > 0)
                    msg += " Gợi ý: " + string.Join(", ", availability.SuggestedDates.Select(d => d.ToString("dd/MM/yyyy")));
                throw new InvalidOperationException(msg);
            }

            var shopOwner = await _orderRepo.GetDefaultShopOwnerAsync();
            var totalAmount = dto.TotalAmount > 0 ? dto.TotalAmount : concept.QuotedAmount;
            if (totalAmount <= 0)
                throw new InvalidOperationException("Báo giá chưa hợp lệ. Vui lòng liên hệ Chủ tiệm.");

            var depositAmount = dto.DepositAmount ?? Math.Round(totalAmount * _settings.DepositPercentage, 0);

            var order = new Order
            {
                OrderCode = GenerateOrderCode(),
                ContactFullName = dto.FullName.Trim(),
                ContactPhone = dto.PhoneNumber.Trim(),
                ContactEmail = dto.Email.Trim(),
                CustomerId = customerId,
                ShopOwnerId = shopOwner?.Id,
                EventTypeId = dto.EventTypeId,
                ConceptId = dto.ConceptId,
                EventName = string.IsNullOrWhiteSpace(dto.EventName) ? concept.Name : dto.EventName.Trim(),
                EventDate = eventDate,
                SetupTime = setupTime,
                Address = dto.Address.Trim(),
                District = dto.District.Trim(),
                TotalAmount = totalAmount,
                DepositAmount = depositAmount,
                Status = OrderStatus.PendingConfirmation,
                Notes = dto.Notes?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepo.AddOrderAsync(order);
            await AddStatusHistoryAsync(order, OrderStatus.PendingConfirmation, customerId,
                "Khách hàng gửi yêu cầu đặt lịch. Chờ Chủ tiệm xác nhận khả năng thi công.");
            await _orderRepo.SaveChangesAsync();

            return await MapOrderDtoAsync(order);
        }

        public async Task<OrderDto?> GetBookingAsync(Guid orderId, Guid customerId)
        {
            var order = await _orderRepo.GetByIdForCustomerAsync(orderId, customerId);
            return order == null ? null : await MapOrderDtoAsync(order);
        }

        public async Task<List<OrderListItemDto>> GetMyBookingsAsync(Guid customerId)
        {
            var orders = await _orderRepo.GetByCustomerIdAsync(customerId);
            return orders.Select(MapListItem).ToList();
        }

        public async Task<OrderDto> TrackBookingAsync(Guid orderId, Guid customerId)
        {
            var order = await _orderRepo.GetByIdForCustomerAsync(orderId, customerId)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");
            return await MapOrderDtoAsync(order);
        }

        public async Task<PaymentDto> CreatePaymentAsync(Guid orderId, Guid customerId, CreatePaymentDto dto)
        {
            var order = await _orderRepo.GetByIdForCustomerAsync(orderId, customerId)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");

            if (order.Status != OrderStatus.WaitingDeposit)
                throw new InvalidOperationException("Chỉ có thể thanh toán đặt cọc khi đơn ở trạng thái Chờ đặt cọc.");

            if (!BookingValidator.IsValidServiceArea(order.Address, order.District, _settings, out _))
                throw new InvalidOperationException("Địa chỉ không thuộc phạm vi phục vụ. Không thể thanh toán.");

            var amount = dto.Amount > 0 ? dto.Amount : order.DepositAmount;
            if (amount < order.DepositAmount * 0.99m)
                throw new InvalidOperationException($"Số tiền đặt cọc tối thiểu là {order.DepositAmount:N0} đ.");

            var method = NormalizePaymentMethod(dto.PaymentMethod);
            var transactionId = string.IsNullOrWhiteSpace(dto.TransactionId)
                ? $"BLM{order.Id.ToString()[..8].ToUpperInvariant()}"
                : dto.TransactionId.Trim();

            var qrUrl = BuildQrCodeUrl(method, amount, order.OrderCode, transactionId);

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = amount,
                PaymentMethod = method,
                TransactionId = transactionId,
                Status = "Success",
                QrCodeUrl = qrUrl,
                CreatedAt = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow
            };

            await _orderRepo.AddPaymentAsync(payment);

            order.Status = OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;
            await AddStatusHistoryAsync(order, OrderStatus.Confirmed, customerId,
                $"Khách hàng đã thanh toán đặt cọc {amount:N0} đ qua {method}. Mã GD: {transactionId}");

            await _orderRepo.SaveChangesAsync();

            return MapPayment(payment);
        }

        public async Task<OrderDto> RescheduleBookingAsync(Guid orderId, Guid customerId, RescheduleBookingDto dto)
        {
            var order = await _orderRepo.GetByIdForCustomerAsync(orderId, customerId)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");

            var allowedReschedule = new[]
            {
                OrderStatus.PendingConfirmation, OrderStatus.WaitingDeposit,
                OrderStatus.Confirmed, OrderStatus.Preparing
            };

            if (!allowedReschedule.Contains(order.Status))
                throw new InvalidOperationException("Không thể đổi lịch ở trạng thái hiện tại.");

            if (!BookingValidator.IsValidServiceArea(dto.NewAddress, dto.NewDistrict, _settings, out var areaError))
                throw new InvalidOperationException(areaError);

            var newDate = dto.NewEventDate.Date;
            if (!BookingValidator.IsFutureOrToday(newDate, out var dateError))
                throw new InvalidOperationException(dateError);

            var newSetupTime = ParseSetupTime(dto.NewSetupTime);
            var availability = await CheckAvailabilityAsync(newDate, newSetupTime, order.Id);
            if (!availability.IsAvailable)
                throw new InvalidOperationException(availability.Message);

            order.EventDate = newDate;
            order.SetupTime = newSetupTime;
            order.Address = dto.NewAddress.Trim();
            order.District = dto.NewDistrict.Trim();
            order.Status = OrderStatus.PendingConfirmation;
            order.UpdatedAt = DateTime.UtcNow;

            await AddStatusHistoryAsync(order, OrderStatus.PendingConfirmation, customerId,
                $"Yêu cầu đổi lịch: {dto.Reason}. Chờ Chủ tiệm xác nhận lại.");

            await _orderRepo.SaveChangesAsync();
            return await MapOrderDtoAsync(order);
        }

        public async Task<OrderDto> CancelBookingAsync(Guid orderId, Guid customerId, CancelBookingDto dto)
        {
            var order = await _orderRepo.GetByIdForCustomerAsync(orderId, customerId)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");

            if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Rejected)
                throw new InvalidOperationException("Không thể hủy đơn ở trạng thái hiện tại.");

            var refundNote = GetRefundPolicyNote(order);
            order.Status = OrderStatus.Cancelled;
            order.CancellationReason = dto.Reason.Trim();
            order.UpdatedAt = DateTime.UtcNow;

            await AddStatusHistoryAsync(order, OrderStatus.Cancelled, customerId,
                $"Khách hàng hủy đơn. {refundNote}");

            await _orderRepo.SaveChangesAsync();
            var result = await MapOrderDtoAsync(order);
            result.RefundPolicyNote = refundNote;
            return result;
        }

        public async Task<OrderDto> SubmitReviewAsync(Guid orderId, Guid customerId, SubmitReviewDto dto)
        {
            var order = await _orderRepo.GetByIdForCustomerAsync(orderId, customerId)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");

            if (order.Status != OrderStatus.Completed)
                throw new InvalidOperationException("Chỉ có thể đánh giá sau khi sự kiện hoàn thành.");

            if (dto.Rating < 1 || dto.Rating > 5)
                throw new InvalidOperationException("Đánh giá từ 1 đến 5 sao.");

            var review = new Review
            {
                OrderId = order.Id,
                CustomerId = customerId,
                Rating = dto.Rating,
                Comment = dto.Comment.Trim(),
                ImageUrls = JsonSerializer.Serialize(dto.ImageUrls ?? new List<string>()),
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepo.AddReviewAsync(review);
            await _orderRepo.SaveChangesAsync();
            return await MapOrderDtoAsync(order);
        }

        public async Task<ShopOwnerDashboardDto> GetShopOwnerDashboardAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var pending = await _orderRepo.GetPendingForShopOwnerAsync();
            var upcoming = await _orderRepo.GetUpcomingSetupsAsync();

            var allRecent = upcoming;
            var todayCount = allRecent.Count(o => o.EventDate.Date == today);
            var weekCount = allRecent.Count(o => o.EventDate >= weekStart);
            var monthRevenue = allRecent
                .Where(o => o.CreatedAt >= monthStart && o.Status >= OrderStatus.Confirmed)
                .Sum(o => o.DepositAmount);

            return new ShopOwnerDashboardDto
            {
                TodayOrders = todayCount,
                WeekOrders = weekCount,
                MonthRevenue = monthRevenue,
                PendingConfirmations = pending.Count,
                PendingBookings = pending.Select(MapListItem).ToList(),
                UpcomingSetups = upcoming.Select(MapListItem).ToList()
            };
        }

        public async Task<List<OrderListItemDto>> GetPendingBookingsAsync()
        {
            var orders = await _orderRepo.GetPendingForShopOwnerAsync();
            return orders.Select(MapListItem).ToList();
        }

        public async Task<OrderDto> ConfirmBookingAsync(Guid orderId, Guid shopOwnerId, ConfirmBookingDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, includeDetails: true)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");

            if (order.Status != OrderStatus.PendingConfirmation)
                throw new InvalidOperationException("Chỉ có thể xác nhận đơn đang chờ xác nhận.");

            if (!dto.Approved)
            {
                order.Status = OrderStatus.Rejected;
                order.CancellationReason = dto.Notes ?? "Chủ tiệm từ chối do không đủ nguồn lực thi công.";
                order.ShopOwnerId = shopOwnerId;
                order.UpdatedAt = DateTime.UtcNow;
                await AddStatusHistoryAsync(order, OrderStatus.Rejected, shopOwnerId, order.CancellationReason);
            }
            else
            {
                order.Status = OrderStatus.WaitingDeposit;
                order.ShopOwnerId = shopOwnerId;
                order.UpdatedAt = DateTime.UtcNow;
                await AddStatusHistoryAsync(order, OrderStatus.WaitingDeposit, shopOwnerId,
                    dto.Notes ?? "Chủ tiệm xác nhận khả năng thi công. Vui lòng thanh toán đặt cọc.");
            }

            await _orderRepo.SaveChangesAsync();
            return await MapOrderDtoAsync(order);
        }

        public async Task<OrderDto> UpdateBookingStatusAsync(Guid orderId, Guid shopOwnerId, UpdateBookingStatusDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, includeDetails: true)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");

            ValidateStatusTransition(order.Status, dto.Status);

            order.Status = dto.Status;
            order.ShopOwnerId = shopOwnerId;
            order.UpdatedAt = DateTime.UtcNow;

            await AddStatusHistoryAsync(order, dto.Status, shopOwnerId,
                dto.Notes ?? StatusLabels.GetValueOrDefault(dto.Status, dto.Status.ToString()));

            await _orderRepo.SaveChangesAsync();
            return await MapOrderDtoAsync(order);
        }

        public async Task<List<OrderListItemDto>> GetUpcomingSetupsAsync()
        {
            var orders = await _orderRepo.GetUpcomingSetupsAsync();
            return orders.Select(MapListItem).ToList();
        }

        public async Task ApproveReviewAsync(Guid reviewId, Guid shopOwnerId, bool approved)
        {
            await Task.CompletedTask;
        }

        public async Task<object> ApproveConceptQuoteAsync(Guid conceptId, Guid customerId, decimal? quotedAmount)
        {
            var concept = await _orderRepo.GetConceptAsync(conceptId)
                ?? throw new InvalidOperationException("Concept không tồn tại.");

            if (concept.CustomerId != null && concept.CustomerId != customerId)
                throw new InvalidOperationException("Concept không thuộc về bạn.");

            if (quotedAmount.HasValue && quotedAmount > 0)
                concept.QuotedAmount = quotedAmount.Value;

            if (concept.QuotedAmount <= 0)
                throw new InvalidOperationException("Báo giá chưa được thiết lập. Vui lòng liên hệ Chủ tiệm.");

            concept.IsQuoteApproved = true;
            concept.CustomerId ??= customerId;
            await _orderRepo.SaveChangesAsync();

            return new
            {
                concept.Id,
                concept.Name,
                concept.QuotedAmount,
                concept.IsQuoteApproved,
                message = "Đã xác nhận báo giá. Bạn có thể tiến hành đặt lịch."
            };
        }

        public async Task<OrderDto> GetBookingForShopOwnerAsync(Guid orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, includeDetails: true)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");
            return await MapOrderDtoAsync(order);
        }

        private async Task AddStatusHistoryAsync(Order order, OrderStatus status, Guid updatedById, string notes)
        {
            var history = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = status,
                Notes = notes,
                UpdatedById = updatedById,
                UpdatedAt = DateTime.UtcNow
            };
            await _orderRepo.AddStatusHistoryAsync(history);
            order.StatusHistory.Add(history);
        }

        private async Task<(bool IsAvailable, string Message, List<DateTime>? SuggestedDates)> CheckAvailabilityAsync(
            DateTime eventDate, TimeSpan setupTime, Guid? excludeOrderId)
        {
            var count = await _orderRepo.CountActiveOrdersOnDateAsync(eventDate, excludeOrderId);
            if (count >= _settings.MaxOrdersPerDay)
            {
                var suggestions = await GetSuggestedDatesAsync(excludeOrderId, 5);
                return (false,
                    $"Đã hết lịch cho ngày {eventDate:dd/MM/yyyy} (tối đa {_settings.MaxOrdersPerDay} đơn/ngày).",
                    suggestions);
            }

            var sameDayOrders = await _orderRepo.GetOrdersOnDateAsync(eventDate, excludeOrderId);
            var duration = TimeSpan.FromHours(_settings.SetupDurationHours);

            foreach (var existing in sameDayOrders)
            {
                var existingStart = existing.SetupTime;
                var existingEnd = existingStart.Add(duration);
                var newStart = setupTime;
                var newEnd = setupTime.Add(duration);

                if (newStart < existingEnd && newEnd > existingStart)
                {
                    var suggestions = await GetSuggestedDatesAsync(excludeOrderId, 5);
                    return (false,
                        $"Trùng khung giờ setup ({existing.SetupTime:hh\\:mm}). Vui lòng chọn giờ hoặc ngày khác.",
                        suggestions);
                }
            }

            return (true, string.Empty, null);
        }

        private async Task<List<DateTime>> GetSuggestedDatesAsync(Guid? excludeOrderId, int count)
        {
            var suggestions = new List<DateTime>();
            var date = DateTime.UtcNow.Date.AddDays(1);

            while (suggestions.Count < count && suggestions.Count < 14)
            {
                var dayCount = await _orderRepo.CountActiveOrdersOnDateAsync(date, excludeOrderId);
                if (dayCount < _settings.MaxOrdersPerDay)
                    suggestions.Add(date);
                date = date.AddDays(1);
            }

            return suggestions;
        }

        private static void ValidateStatusTransition(OrderStatus current, OrderStatus next)
        {
            var allowed = current switch
            {
                OrderStatus.Confirmed => new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
                OrderStatus.Preparing => new[] { OrderStatus.Transporting, OrderStatus.Cancelled },
                OrderStatus.Transporting => new[] { OrderStatus.SettingUp },
                OrderStatus.SettingUp => new[] { OrderStatus.Completed },
                OrderStatus.WaitingDeposit => new[] { OrderStatus.Cancelled },
                _ => Array.Empty<OrderStatus>()
            };

            if (current == next) return;

            if (!allowed.Contains(next))
                throw new InvalidOperationException($"Không thể chuyển từ {current} sang {next}.");
        }

        private static string GetRefundPolicyNote(Order order)
        {
            var hasDeposit = order.Status >= OrderStatus.Confirmed;
            var daysUntilEvent = (order.EventDate.Date - DateTime.UtcNow.Date).Days;

            if (!hasDeposit)
                return "Chính sách hoàn cọc: Chưa thanh toán cọc — không áp dụng hoàn tiền.";

            if (daysUntilEvent >= 7)
                return "Chính sách hoàn cọc: Hoàn 100% tiền cọc (hủy trước 7 ngày).";
            if (daysUntilEvent >= 3)
                return "Chính sách hoàn cọc: Hoàn 50% tiền cọc (hủy trước 3–6 ngày).";
            return "Chính sách hoàn cọc: Không hoàn cọc (hủy trong vòng 2 ngày trước sự kiện).";
        }

        private async Task<OrderDto> MapOrderDtoAsync(Order order)
        {
            if (order.EventType == null && order.EventTypeId.HasValue)
                order = await _orderRepo.GetByIdAsync(order.Id, includeDetails: true) ?? order;

            return new OrderDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                FullName = order.ContactFullName,
                PhoneNumber = order.ContactPhone,
                Email = order.ContactEmail,
                EventName = order.EventName,
                EventTypeId = order.EventTypeId,
                EventTypeName = order.EventType?.Name,
                ConceptId = order.ConceptId,
                ConceptName = order.Concept?.Name,
                EventDate = order.EventDate,
                SetupTime = order.SetupTime.ToString(@"hh\:mm"),
                Address = order.Address,
                District = order.District,
                TotalAmount = order.TotalAmount,
                DepositAmount = order.DepositAmount,
                Status = order.Status,
                StatusLabel = StatusLabels.GetValueOrDefault(order.Status, order.Status.ToString()),
                Notes = order.Notes,
                CancellationReason = order.CancellationReason,
                CreatedAt = order.CreatedAt,
                Payments = order.Payments?.Select(MapPayment).ToList() ?? new(),
                StatusHistory = order.StatusHistory?
                    .OrderBy(h => h.UpdatedAt)
                    .Select(h => new OrderStatusHistoryDto
                    {
                        Status = h.Status,
                        Notes = h.Notes,
                        UpdatedAt = h.UpdatedAt
                    }).ToList() ?? new()
            };
        }

        private OrderListItemDto MapListItem(Order order) => new()
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            EventName = order.EventName,
            EventDate = order.EventDate,
            District = order.District,
            Status = order.Status,
            StatusLabel = StatusLabels.GetValueOrDefault(order.Status, order.Status.ToString()),
            TotalAmount = order.TotalAmount,
            DepositAmount = order.DepositAmount
        };

        private static PaymentDto MapPayment(Payment p) => new()
        {
            Id = p.Id,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            TransactionId = p.TransactionId,
            Status = p.Status,
            QrCodeUrl = p.QrCodeUrl,
            CreatedAt = p.CreatedAt,
            PaidAt = p.PaidAt
        };

        private static TimeSpan ParseSetupTime(string setupTime)
        {
            if (TimeSpan.TryParse(setupTime, out var ts))
                return ts;
            throw new InvalidOperationException("Giờ setup không hợp lệ.");
        }

        private static string GenerateOrderCode()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"BLM-{date}-{suffix}";
        }

        private static string NormalizePaymentMethod(string method) => method switch
        {
            "Momo" or "QRCode" or "VNPay" or "BankTransfer" => method,
            _ => "BankTransfer"
        };

        private static string BuildQrCodeUrl(string method, decimal amount, string orderCode, string transactionId)
        {
            var note = Uri.EscapeDataString($"Bloomy_{orderCode}_{transactionId}");
            var amt = (int)amount;
            return method switch
            {
                "Momo" => $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=momo://transfer?phone=0987654321&amount={amt}&note={note}",
                "QRCode" => $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=vietqr://payment?bank=MBBank&account=19028372615&amount={amt}&note={note}",
                _ => $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=transfer?amount={amt}&note={note}"
            };
        }
    }
}
