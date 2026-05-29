using Bloomy.Models;
using Bloomy.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bloomy.Data
{
    public class BloomyDbContext : DbContext
    {
        public BloomyDbContext(DbContextOptions<BloomyDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<Concept> Concepts { get; set; }
        public DbSet<ConceptImage> ConceptImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<PortfolioImage> PortfolioImages { get; set; }
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<BrandSetting> BrandSettings { get; set; }
        public DbSet<PaymentSetting> PaymentSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PaymentSetting>()
                .ToTable("PaymentSettings")
                .Property(p => p.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<BrandSetting>()
                .ToTable("BrandSettings");

            // Index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // ==================== CONCEPT - ORDER ====================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Concept)
                .WithMany()
               .HasForeignKey(o => o.ConceptId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);


            // User -> Order (Customer)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // User -> Order (ShopOwner)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShopOwner)
                .WithMany(u => u.ManagedOrders)
                .HasForeignKey(o => o.ShopOwnerId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // OrderStatusHistory
            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.UpdatedBy)
                .WithMany()
                .HasForeignKey(h => h.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ConceptImage
            modelBuilder.Entity<ConceptImage>()
                .HasOne(ci => ci.Concept)
                .WithMany(c => c.Images)
                .HasForeignKey(ci => ci.ConceptId)
                .OnDelete(DeleteBehavior.Cascade);

            // PortfolioImage
            modelBuilder.Entity<PortfolioImage>()
                .HasOne(pi => pi.PortfolioItem)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PortfolioItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatConversation
            modelBuilder.Entity<ChatConversation>()
                .HasOne(cc => cc.Customer)
                .WithMany(u => u.ConversationsAsCustomer)
                .HasForeignKey(cc => cc.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<ChatConversation>()
                .HasOne(cc => cc.ShopOwner)
                .WithMany(u => u.ConversationsAsShopOwner)
                .HasForeignKey(cc => cc.ShopOwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<ChatConversation>()
                .HasOne(cc => cc.Order)
                .WithMany()
                .HasForeignKey(cc => cc.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // ChatMessage
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Conversation)
                .WithMany()
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ServicePackage -> EventType
            modelBuilder.Entity<ServicePackage>()
                .HasOne(sp => sp.EventType)
                .WithMany()
                .HasForeignKey(sp => sp.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // PortfolioItem -> EventType
            modelBuilder.Entity<PortfolioItem>()
                .HasOne(pi => pi.EventType)
                .WithMany()
                .HasForeignKey(pi => pi.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Global Query Filter cho User
            modelBuilder.Entity<User>().HasQueryFilter(u => u.IsActive);
        }
    }
}