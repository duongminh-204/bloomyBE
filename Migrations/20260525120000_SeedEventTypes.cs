using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    public partial class SeedEventTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM EventTypes)
BEGIN
    SET IDENTITY_INSERT EventTypes ON;
    INSERT INTO EventTypes (Id, Name, Description, IconUrl) VALUES
    (1, N'Sinh nhật', N'Trang trí tiệc sinh nhật', N''),
    (2, N'Tiệc cưới', N'Trang trí tiệc cưới, lễ cưới', N''),
    (3, N'Khai trương', N'Trang trí khai trương, kỷ niệm', N''),
    (4, N'Hội nghị', N'Trang trí hội nghị, sự kiện doanh nghiệp', N''),
    (5, N'Baby shower', N'Trang trí tiệc baby shower', N'');
    SET IDENTITY_INSERT EventTypes OFF;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM EventTypes WHERE Id BETWEEN 1 AND 5");
        }
    }
}
