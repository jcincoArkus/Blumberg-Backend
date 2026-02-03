using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentHasSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "equipment_id",
                table: "sensors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_sensors_equipment_id",
                table: "sensors",
                column: "equipment_id");

            migrationBuilder.AddForeignKey(
                name: "fk_sensors_equipment_id",
                table: "sensors",
                column: "equipment_id",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sensors_equipment_id",
                table: "sensors");

            migrationBuilder.DropIndex(
                name: "ix_sensors_equipment_id",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "equipment_id",
                table: "sensors");
        }
    }
}
