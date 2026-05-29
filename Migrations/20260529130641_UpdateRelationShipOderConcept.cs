using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationShipOderConcept : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_ConceptId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Concepts");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ConceptId",
                table: "Orders",
                column: "ConceptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_ConceptId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Concepts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ConceptId",
                table: "Orders",
                column: "ConceptId",
                unique: true,
                filter: "[ConceptId] IS NOT NULL");
        }
    }
}
