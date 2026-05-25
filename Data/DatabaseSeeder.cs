using Bloomy.Data;
using Bloomy.Models;
using Microsoft.EntityFrameworkCore;

namespace BloomyBE.Data
{
    public static class DatabaseSeeder
    {
        public static readonly EventType[] DefaultEventTypes =
        [
            new() { Name = "Sinh nhật", Description = "Trang trí tiệc sinh nhật", IconUrl = "" },
            new() { Name = "Tiệc cưới", Description = "Trang trí tiệc cưới, lễ cưới", IconUrl = "" },
            new() { Name = "Khai trương", Description = "Trang trí khai trương, kỷ niệm", IconUrl = "" },
            new() { Name = "Hội nghị", Description = "Trang trí hội nghị, sự kiện doanh nghiệp", IconUrl = "" },
            new() { Name = "Baby shower", Description = "Trang trí tiệc baby shower", IconUrl = "" },
        ];

        public static async Task SeedAsync(BloomyDbContext context)
        {
            await EnsurePaymentSettingsTableAsync(context);

            if (await context.EventTypes.AnyAsync())
                return;

            await context.EventTypes.AddRangeAsync(DefaultEventTypes);
            await context.SaveChangesAsync();
        }

        private static async Task EnsurePaymentSettingsTableAsync(BloomyDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentSettings')
                BEGIN
                    CREATE TABLE PaymentSettings (
                        Id int NOT NULL PRIMARY KEY,
                        QrImageUrl nvarchar(max) NOT NULL DEFAULT '',
                        AccountHolderName nvarchar(200) NOT NULL,
                        AccountNumber nvarchar(50) NOT NULL,
                        BankName nvarchar(150) NOT NULL,
                        TransferContentTemplate nvarchar(200) NOT NULL DEFAULT 'BLM DEPOSIT {OrderCode}',
                        MomoPhone nvarchar(20) NOT NULL DEFAULT '',
                        EnableMomo bit NOT NULL DEFAULT 1,
                        EnableQrCode bit NOT NULL DEFAULT 1,
                        EnableBankTransfer bit NOT NULL DEFAULT 1,
                        EnableVNPay bit NOT NULL DEFAULT 0,
                        UpdatedAt datetime2 NOT NULL
                    );
                    INSERT INTO PaymentSettings (Id, QrImageUrl, AccountHolderName, AccountNumber, BankName, TransferContentTemplate, MomoPhone, EnableMomo, EnableQrCode, EnableBankTransfer, EnableVNPay, UpdatedAt)
                    VALUES (1, '', N'CÔNG TY TNHH BLOOMY VIỆT NAM', '19028372615', N'MB Bank (Ngân hàng Quân Đội)', 'BLM DEPOSIT {OrderCode}', '0987654321', 1, 1, 1, 0, GETUTCDATE());
                END");
        }
    }
}