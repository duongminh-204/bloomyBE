using Bloomy.DTOs;

namespace BloomyBE.Services.Interfaces
{
    public interface IPaymentSettingsService
    {
        Task<PaymentSettingsDto> GetAsync();
        Task<PaymentSettingsDto> UpdateAsync(UpdatePaymentSettingsDto dto);
        Task<string> SaveQrImageAsync(Stream fileStream, string fileName);
        string BuildTransferContent(string template, string orderCode, string transactionId, decimal amount);
    }
}
