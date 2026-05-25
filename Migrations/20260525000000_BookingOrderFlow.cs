using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    public partial class BookingOrderFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactFullName",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Orders",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "QuotedAmount",
                table: "Concepts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsQuoteApproved",
                table: "Concepts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "QrCodeUrl",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "OrderCode", table: "Orders");
            migrationBuilder.DropColumn(name: "ContactFullName", table: "Orders");
            migrationBuilder.DropColumn(name: "ContactPhone", table: "Orders");
            migrationBuilder.DropColumn(name: "ContactEmail", table: "Orders");
            migrationBuilder.DropColumn(name: "QuotedAmount", table: "Concepts");
            migrationBuilder.DropColumn(name: "IsQuoteApproved", table: "Concepts");
            migrationBuilder.DropColumn(name: "ImageUrls", table: "Reviews");
            migrationBuilder.DropColumn(name: "QrCodeUrl", table: "Payments");
            migrationBuilder.DropColumn(name: "PaidAt", table: "Payments");
        }
    }
}
