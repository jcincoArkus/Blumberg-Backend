using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class SensorHasReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sensor_id",
                table: "sensor_readings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_sensor_id",
                table: "sensor_readings",
                column: "sensor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_sensor_readings_sensor_id",
                table: "sensor_readings",
                column: "sensor_id",
                principalTable: "sensors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sensor_readings_sensor_id",
                table: "sensor_readings");

            migrationBuilder.DropIndex(
                name: "ix_sensor_readings_sensor_id",
                table: "sensor_readings");

            migrationBuilder.DropColumn(
                name: "sensor_id",
                table: "sensor_readings");
        }
    }
}
