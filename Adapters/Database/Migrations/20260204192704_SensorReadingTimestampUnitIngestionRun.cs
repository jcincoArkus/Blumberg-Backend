using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class SensorReadingTimestampUnitIngestionRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ingestion_run_id",
                table: "sensor_readings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp_utc",
                table: "sensor_readings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "unit",
                table: "sensor_readings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ingestion_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingestion_runs_id", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sensor_readings_ingestion_run_id",
                table: "sensor_readings",
                column: "ingestion_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_organization_id_timestamp_utc",
                table: "sensor_readings",
                columns: new[] { "organization_id", "timestamp_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_sensor_id_timestamp_utc",
                table: "sensor_readings",
                columns: new[] { "sensor_id", "timestamp_utc" });

            migrationBuilder.AddForeignKey(
                name: "fk_sensor_readings_ingestion_run_id",
                table: "sensor_readings",
                column: "ingestion_run_id",
                principalTable: "ingestion_runs",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sensor_readings_ingestion_run_id",
                table: "sensor_readings");

            migrationBuilder.DropTable(
                name: "ingestion_runs");

            migrationBuilder.DropIndex(
                name: "IX_sensor_readings_ingestion_run_id",
                table: "sensor_readings");

            migrationBuilder.DropIndex(
                name: "ix_sensor_readings_organization_id_timestamp_utc",
                table: "sensor_readings");

            migrationBuilder.DropIndex(
                name: "ix_sensor_readings_sensor_id_timestamp_utc",
                table: "sensor_readings");

            migrationBuilder.DropColumn(
                name: "ingestion_run_id",
                table: "sensor_readings");

            migrationBuilder.DropColumn(
                name: "timestamp_utc",
                table: "sensor_readings");

            migrationBuilder.DropColumn(
                name: "unit",
                table: "sensor_readings");
        }
    }
}
