namespace Bloomy.DTOs
{
    public class PaymentSettingsDto
    {
        public string QrImageUrl { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string TransferContentTemplate { get; set; } = string.Empty;
        public string MomoPhone { get; set; } = string.Empty;
        public bool EnableMomo { get; set; }
        public bool EnableQrCode { get; set; }
        public bool EnableBankTransfer { get; set; }
        public bool EnableVNPay { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdatePaymentSettingsDto
    {
        public string QrImageUrl { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string TransferContentTemplate { get; set; } = string.Empty;
        public string MomoPhone { get; set; } = string.Empty;
        public bool EnableMomo { get; set; } = true;
        public bool EnableQrCode { get; set; } = true;
        public bool EnableBankTransfer { get; set; } = true;
        public bool EnableVNPay { get; set; }
    }
}
