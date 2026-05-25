using Bloomy.DTOs.Orders;

namespace BloomyBE.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateBookingAsync(Guid customerId, CreateBookingDto dto);
        Task<OrderDto?> GetBookingAsync(Guid orderId, Guid customerId);
        Task<List<OrderListItemDto>> GetMyBookingsAsync(Guid customerId);
        Task<OrderDto> TrackBookingAsync(Guid orderId, Guid customerId);
        Task<PaymentDto> CreatePaymentAsync(Guid orderId, Guid customerId, CreatePaymentDto dto);
        Task<OrderDto> RescheduleBookingAsync(Guid orderId, Guid customerId, RescheduleBookingDto dto);
        Task<OrderDto> CancelBookingAsync(Guid orderId, Guid customerId, CancelBookingDto dto);
        Task<OrderDto> SubmitReviewAsync(Guid orderId, Guid customerId, SubmitReviewDto dto);
        Task<ShopOwnerDashboardDto> GetShopOwnerDashboardAsync();
        Task<List<OrderListItemDto>> GetPendingBookingsAsync();
        Task<OrderDto> ConfirmBookingAsync(Guid orderId, Guid shopOwnerId, ConfirmBookingDto dto);
        Task<OrderDto> UpdateBookingStatusAsync(Guid orderId, Guid shopOwnerId, UpdateBookingStatusDto dto);
        Task<List<OrderListItemDto>> GetUpcomingSetupsAsync();
        Task ApproveReviewAsync(Guid reviewId, Guid shopOwnerId, bool approved);
        Task<object> ApproveConceptQuoteAsync(Guid conceptId, Guid customerId, decimal? quotedAmount);
        Task<OrderDto> GetBookingForShopOwnerAsync(Guid orderId);
        Task<PaymentDto> ConfirmPaymentAsync(Guid orderId, Guid paymentId, Guid shopOwnerId);
        Task<List<PendingPaymentOrderDto>> GetPendingPaymentConfirmationsAsync();
    }
}
