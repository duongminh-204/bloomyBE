using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    public partial class AddPaymentSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    QrImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountHolderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TransferContentTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MomoPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnableMomo = table.Column<bool>(type: "bit", nullable: false),
                    EnableQrCode = table.Column<bool>(type: "bit", nullable: false),
                    EnableBankTransfer = table.Column<bool>(type: "bit", nullable: false),
                    EnableVNPay = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PaymentSettings",
                columns: new[] { "Id", "QrImageUrl", "AccountHolderName", "AccountNumber", "BankName", "TransferContentTemplate", "MomoPhone", "EnableMomo", "EnableQrCode", "EnableBankTransfer", "EnableVNPay", "UpdatedAt" },
                values: new object[] { 1, "", "CÔNG TY TNHH BLOOMY VIỆT NAM", "19028372615", "MB Bank (Ngân hàng Quân Đội)", "BLM DEPOSIT {OrderCode}", "0987654321", true, true, true, false, DateTime.UtcNow });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PaymentSettings");
        }
    }
}
