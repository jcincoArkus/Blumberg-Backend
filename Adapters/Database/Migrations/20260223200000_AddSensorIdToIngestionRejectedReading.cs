using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorIdToIngestionRejectedReading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sensor_id",
                table: "ingestion_run_rejected_readings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_ingestion_run_rejected_readings_sensor_id",
                table: "ingestion_run_rejected_readings",
                column: "sensor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_ingestion_run_rejected_readings_sensor_id",
                table: "ingestion_run_rejected_readings");

            migrationBuilder.DropColumn(
                name: "sensor_id",
                table: "ingestion_run_rejected_readings");
        }
    }
}
