using Bloomy.Models;
using Bloomy.Models.Enums;

namespace Bloomy.Data.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, bool includeDetails = false);
        Task<Order?> GetByIdForCustomerAsync(Guid id, Guid customerId);
        Task<List<Order>> GetByCustomerIdAsync(Guid customerId);
        Task<List<Order>> GetPendingForShopOwnerAsync();
        Task<List<Order>> GetManagedOrdersAsync();
        Task<List<Order>> GetUpcomingSetupsAsync(int days = 14);
        Task<List<Order>> GetCalendarOrdersAsync(DateTime from, DateTime to);
        Task<int> CountActiveOrdersOnDateAsync(DateTime date, Guid? excludeOrderId = null);
        Task<List<Order>> GetOrdersOnDateAsync(DateTime date, Guid? excludeOrderId = null);
        Task<User?> GetDefaultShopOwnerAsync();
        Task<Concept?> GetConceptAsync(Guid conceptId);
        Task<EventType?> GetEventTypeByIdAsync(int eventTypeId);
        Task<EventType?> GetDefaultEventTypeAsync();
        Task<Concept> CreateConceptAsync(Concept concept);
        Task<List<Concept>> GetConceptsByCustomerAsync(Guid customerId);
        Task DeleteConceptAsync(Guid conceptId);
        Task AddOrderAsync(Order order);
        Task AddStatusHistoryAsync(OrderStatusHistory history);
        Task AddPaymentAsync(Payment payment);
        Task<Payment?> GetPaymentAsync(Guid paymentId);
        Task AddReviewAsync(Review review);
        Task<List<Order>> GetOrdersWithPendingPaymentsAsync();
        Task SaveChangesAsync();
    }
}
