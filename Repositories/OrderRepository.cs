using Bloomy.Data.Interfaces;
using Bloomy.Models;
using Bloomy.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bloomy.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly BloomyDbContext _context;

        private static readonly OrderStatus[] ActiveStatuses =
        [
            OrderStatus.PendingConfirmation,
            OrderStatus.WaitingDeposit,
            OrderStatus.Confirmed,
            OrderStatus.Preparing,
            OrderStatus.Transporting,
            OrderStatus.SettingUp
        ];

        public OrderRepository(BloomyDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(Guid id, bool includeDetails = false)
        {
            var query = _context.Orders.AsQueryable();
            if (includeDetails)
            {
                query = query
                    .Include(o => o.Customer)
                    .Include(o => o.EventType)
                    .Include(o => o.Concept)
                    .Include(o => o.Payments)
                    .Include(o => o.StatusHistory)
                        .ThenInclude(h => h.UpdatedBy);
            }
            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByIdForCustomerAsync(Guid id, Guid customerId)
        {
            return await _context.Orders
                .Include(o => o.EventType)
                .Include(o => o.Concept)
                .Include(o => o.Payments)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);
        }

        public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetPendingForShopOwnerAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Status == OrderStatus.PendingConfirmation)
                .OrderBy(o => o.EventDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetUpcomingSetupsAsync(int days = 14)
        {
            var from = DateTime.UtcNow.Date;
            var to = from.AddDays(days);
            return await _context.Orders
                .Include(o => o.EventType)
                .Where(o => o.EventDate >= from && o.EventDate <= to
                    && ActiveStatuses.Contains(o.Status))
                .OrderBy(o => o.EventDate)
                .ThenBy(o => o.SetupTime)
                .ToListAsync();
        }

        public async Task<int> CountActiveOrdersOnDateAsync(DateTime date, Guid? excludeOrderId = null)
        {
            var query = _context.Orders.Where(o =>
                o.EventDate.Date == date.Date &&
                ActiveStatuses.Contains(o.Status));

            if (excludeOrderId.HasValue)
                query = query.Where(o => o.Id != excludeOrderId.Value);

            return await query.CountAsync();
        }

        public async Task<List<Order>> GetOrdersOnDateAsync(DateTime date, Guid? excludeOrderId = null)
        {
            var query = _context.Orders.Where(o =>
                o.EventDate.Date == date.Date &&
                ActiveStatuses.Contains(o.Status));

            if (excludeOrderId.HasValue)
                query = query.Where(o => o.Id != excludeOrderId.Value);

            return await query.ToListAsync();
        }

        public async Task<User?> GetDefaultShopOwnerAsync()
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.ShopOwner && u.IsActive);
        }

        public async Task<Concept?> GetConceptAsync(Guid conceptId)
        {
            return await _context.Concepts.FirstOrDefaultAsync(c => c.Id == conceptId);
        }

        public async Task<EventType?> GetEventTypeByIdAsync(int eventTypeId)
        {
            return await _context.EventTypes.FirstOrDefaultAsync(e => e.Id == eventTypeId);
        }

        public async Task<EventType?> GetDefaultEventTypeAsync()
        {
            return await _context.EventTypes.OrderBy(e => e.Id).FirstOrDefaultAsync();
        }

        public async Task<Concept> CreateConceptAsync(Concept concept)
        {
            await _context.Concepts.AddAsync(concept);
            await _context.SaveChangesAsync();
            return concept;
        }

        public async Task<List<Concept>> GetConceptsByCustomerAsync(Guid customerId)
        {
            return await _context.Concepts
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task AddStatusHistoryAsync(OrderStatusHistory history)
        {
            await _context.OrderStatusHistories.AddAsync(history);
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
