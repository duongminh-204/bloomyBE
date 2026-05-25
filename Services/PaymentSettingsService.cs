using Bloomy.Data;
using Bloomy.DTOs;
using Bloomy.Models;
using BloomyBE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BloomyBE.Services
{
    public class PaymentSettingsService : IPaymentSettingsService
    {
        private readonly BloomyDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PaymentSettingsService(BloomyDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<PaymentSettingsDto> GetAsync()
        {
            var setting = await GetOrCreateAsync();
            return Map(setting);
        }

        public async Task<PaymentSettingsDto> UpdateAsync(UpdatePaymentSettingsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AccountHolderName)
                || string.IsNullOrWhiteSpace(dto.AccountNumber)
                || string.IsNullOrWhiteSpace(dto.BankName))
                throw new InvalidOperationException("Vui lòng nhập đầy đủ tên chủ tài khoản, số tài khoản và tên ngân hàng.");

            var setting = await GetOrCreateAsync();
            setting.QrImageUrl = dto.QrImageUrl?.Trim() ?? string.Empty;
            setting.AccountHolderName = dto.AccountHolderName.Trim();
            setting.AccountNumber = dto.AccountNumber.Trim();
            setting.BankName = dto.BankName.Trim();
            setting.TransferContentTemplate = string.IsNullOrWhiteSpace(dto.TransferContentTemplate)
                ? "BLM DEPOSIT {OrderCode}"
                : dto.TransferContentTemplate.Trim();
            setting.MomoPhone = dto.MomoPhone?.Trim() ?? string.Empty;
            setting.EnableMomo = dto.EnableMomo;
            setting.EnableQrCode = dto.EnableQrCode;
            setting.EnableBankTransfer = dto.EnableBankTransfer;
            setting.EnableVNPay = dto.EnableVNPay;
            setting.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Map(setting);
        }

        public async Task<string> SaveQrImageAsync(Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp" and not ".gif")
                throw new InvalidOperationException("Chỉ hỗ trợ ảnh JPG, PNG, WEBP hoặc GIF.");

            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "payment-qr");
            Directory.CreateDirectory(uploadsDir);

            var safeName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, safeName);

            await using var fs = File.Create(fullPath);
            await fileStream.CopyToAsync(fs);

            return $"/uploads/payment-qr/{safeName}";
        }

        public string BuildTransferContent(string template, string orderCode, string transactionId, decimal amount)
        {
            var t = string.IsNullOrWhiteSpace(template) ? "BLM DEPOSIT {OrderCode}" : template;
            return t
                .Replace("{OrderCode}", orderCode, StringComparison.OrdinalIgnoreCase)
                .Replace("{TransactionId}", transactionId, StringComparison.OrdinalIgnoreCase)
                .Replace("{Amount}", ((int)amount).ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private async Task<PaymentSetting> GetOrCreateAsync()
        {
            var setting = await _db.PaymentSettings.FindAsync(1);
            if (setting != null)
                return setting;

            setting = new PaymentSetting
            {
                Id = 1,
                AccountHolderName = "CÔNG TY TNHH BLOOMY VIỆT NAM",
                AccountNumber = "19028372615",
                BankName = "MB Bank (Ngân hàng Quân Đội)",
                TransferContentTemplate = "BLM DEPOSIT {OrderCode}",
                MomoPhone = "0987654321",
                UpdatedAt = DateTime.UtcNow
            };
            _db.PaymentSettings.Add(setting);
            await _db.SaveChangesAsync();
            return setting;
        }

        private static PaymentSettingsDto Map(PaymentSetting s) => new()
        {
            QrImageUrl = s.QrImageUrl,
            AccountHolderName = s.AccountHolderName,
            AccountNumber = s.AccountNumber,
            BankName = s.BankName,
            TransferContentTemplate = s.TransferContentTemplate,
            MomoPhone = s.MomoPhone,
            EnableMomo = s.EnableMomo,
            EnableQrCode = s.EnableQrCode,
            EnableBankTransfer = s.EnableBankTransfer,
            EnableVNPay = s.EnableVNPay,
            UpdatedAt = s.UpdatedAt
        };
    }
}
