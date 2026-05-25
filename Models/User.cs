using Bloomy.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }

        public UserRole Role { get; set; } = UserRole.Customer;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Order> CustomerOrders { get; set; } = new List<Order>();
        public ICollection<Order> ManagedOrders { get; set; } = new List<Order>();
        public ICollection<ChatConversation> ConversationsAsCustomer { get; set; } = new List<ChatConversation>();
        public ICollection<ChatConversation> ConversationsAsShopOwner { get; set; } = new List<ChatConversation>();
    }
}