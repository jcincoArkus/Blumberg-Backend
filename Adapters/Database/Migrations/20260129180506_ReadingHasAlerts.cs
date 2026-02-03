using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class ReadingHasAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sensor_reading_id",
                table: "alerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_reading_id",
                table: "alerts",
                column: "sensor_reading_id");

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_sensor_reading_id",
                table: "alerts",
                column: "sensor_reading_id",
                principalTable: "sensor_readings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alerts_sensor_reading_id",
                table: "alerts");

            migrationBuilder.DropIndex(
                name: "ix_alerts_sensor_reading_id",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "sensor_reading_id",
                table: "alerts");
        }
    }
}
