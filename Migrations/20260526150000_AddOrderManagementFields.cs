using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    public partial class AddOrderManagementFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousEventDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PreviousSetupTime",
                table: "Orders",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousAddress",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousDistrict",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusBeforeRequest",
                table: "Orders",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "InternalNotes", table: "Orders");
            migrationBuilder.DropColumn(name: "PreviousEventDate", table: "Orders");
            migrationBuilder.DropColumn(name: "PreviousSetupTime", table: "Orders");
            migrationBuilder.DropColumn(name: "PreviousAddress", table: "Orders");
            migrationBuilder.DropColumn(name: "PreviousDistrict", table: "Orders");
            migrationBuilder.DropColumn(name: "StatusBeforeRequest", table: "Orders");
        }
    }
}
