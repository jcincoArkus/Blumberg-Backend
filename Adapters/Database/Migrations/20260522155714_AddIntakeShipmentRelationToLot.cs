using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIntakeShipmentRelationToLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "intake_shipment_id",
                table: "lots",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_lots_intake_shipment_id",
                table: "lots",
                column: "intake_shipment_id");

            migrationBuilder.AddForeignKey(
                name: "fk_lots_intake_shipment_id",
                table: "lots",
                column: "intake_shipment_id",
                principalTable: "intake_shipments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_lots_intake_shipment_id",
                table: "lots");

            migrationBuilder.DropIndex(
                name: "IX_lots_intake_shipment_id",
                table: "lots");

            migrationBuilder.DropColumn(
                name: "intake_shipment_id",
                table: "lots");
        }
    }
}
