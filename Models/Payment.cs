using System.ComponentModel.DataAnnotations.Schema;

namespace Bloomy.Models
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty; // QRCode, Momo, VNPay, BankTransfer
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        public string QrCodeUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}