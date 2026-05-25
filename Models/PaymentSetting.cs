using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bloomy.Models
{
    /// <summary>Singleton (Id = 1) — thông tin nhận tiền do Chủ tiệm quản lý.</summary>
    public class PaymentSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } = 1;

        /// <summary>URL ảnh mã QR ngân hàng do Chủ tiệm upload.</summary>
        public string QrImageUrl { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string BankName { get; set; } = string.Empty;

        /// <summary>Mẫu nội dung CK. Placeholders: {OrderCode}, {TransactionId}, {Amount}</summary>
        [MaxLength(200)]
        public string TransferContentTemplate { get; set; } = "BLM DEPOSIT {OrderCode}";

        [MaxLength(20)]
        public string MomoPhone { get; set; } = string.Empty;

        public bool EnableMomo { get; set; } = true;
        public bool EnableQrCode { get; set; } = true;
        public bool EnableBankTransfer { get; set; } = true;
        public bool EnableVNPay { get; set; } = false;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
