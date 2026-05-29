using Bloomy.Models.Enums;

namespace Bloomy.DTOs.Orders
{
    public class CreateBookingDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int? EventTypeId { get; set; }
        public Guid? ConceptId { get; set; }
        // Accept eventDate as string (YYYY-MM-DD format) and let service handle parsing
        public string EventDate { get; set; } = string.Empty;
        public string SetupTime { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal? DepositAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CreatePaymentDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }

    public class RescheduleBookingDto
    {
        // Accept NewEventDate as string (YYYY-MM-DD format)
        public string NewEventDate { get; set; } = string.Empty;
        public string NewSetupTime { get; set; } = string.Empty;
        public string NewAddress { get; set; } = string.Empty;
        public string NewDistrict { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class CancelBookingDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class SubmitReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public List<string>? ImageUrls { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        public OrderStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ConfirmBookingDto
    {
        public bool Approved { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateInternalNotesDto
    {
        public string InternalNotes { get; set; } = string.Empty;
    }

    public class ShopOwnerRescheduleDto
    {
        // Accept EventDate as string (YYYY-MM-DD format)
        public string EventDate { get; set; } = string.Empty;
        public string SetupTime { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class HandleCustomerRequestDto
    {
        public bool Approved { get; set; }
        public string? Notes { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int? EventTypeId { get; set; }
        public string? EventTypeName { get; set; }
        public Guid? ConceptId { get; set; }
        public string? ConceptName { get; set; }
        public DateTime EventDate { get; set; }
        public string SetupTime { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string InternalNotes { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
        public OrderStatus? StatusBeforeRequest { get; set; }
        public bool HasPendingReschedule { get; set; }
        public bool HasPendingCancel { get; set; }
        public string PaymentStatusSummary { get; set; } = string.Empty;
        public string? RefundPolicyNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PaymentDto> Payments { get; set; } = new();
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
        public List<string>? SuggestedDates { get; set; }
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class OrderStatusHistoryDto
    {
        public OrderStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderListItemDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string? ConceptName { get; set; }
        public DateTime EventDate { get; set; }
        public string SetupTime { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public bool HasDepositPaid { get; set; }
        public bool HasPendingReschedule { get; set; }
        public bool HasPendingCancel { get; set; }
    }

    public class CalendarEventDto
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string SetupTime { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
    }

    public class ShopOwnerDashboardDto
    {
        public int TodayOrders { get; set; }
        public int WeekOrders { get; set; }
        public decimal MonthRevenue { get; set; }
        public int PendingConfirmations { get; set; }
        public int PendingPaymentConfirmations { get; set; }
        public List<OrderListItemDto> PendingBookings { get; set; } = new();
        public List<OrderListItemDto> ManagedBookings { get; set; } = new();
        public List<OrderListItemDto> UpcomingSetups { get; set; } = new();
        public List<PendingPaymentOrderDto> PendingPayments { get; set; } = new();
    }

    public class PendingPaymentOrderDto
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal DepositAmount { get; set; }
        public Guid PaymentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}
