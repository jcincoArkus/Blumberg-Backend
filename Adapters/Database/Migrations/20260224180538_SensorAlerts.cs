using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class SensorAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alerts_sensor_reading_id",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "description",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "title",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "type",
                table: "alerts");

            migrationBuilder.RenameColumn(
                name: "sensor_reading_id",
                table: "alerts",
                newName: "site_id");

            migrationBuilder.RenameIndex(
                name: "ix_alerts_sensor_reading_id",
                table: "alerts",
                newName: "IX_alerts_site_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "alerts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "equipment_id",
                table: "alerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "alerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "resolved_at",
                table: "alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "sensor_id",
                table: "alerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "severity",
                table: "alerts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "threshold_max",
                table: "alerts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "threshold_min",
                table: "alerts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "triggered_at",
                table: "alerts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "triggered_value",
                table: "alerts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_alerts_equipment_id",
                table: "alerts",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_organization_id",
                table: "alerts",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_id",
                table: "alerts",
                column: "sensor_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_id_status",
                table: "alerts",
                columns: new[] { "sensor_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_equipment_id",
                table: "alerts",
                column: "equipment_id",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_organization_id",
                table: "alerts",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_sensor_id",
                table: "alerts",
                column: "sensor_id",
                principalTable: "sensors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_site_id",
                table: "alerts",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alerts_equipment_id",
                table: "alerts");

            migrationBuilder.DropForeignKey(
                name: "fk_alerts_organization_id",
                table: "alerts");

            migrationBuilder.DropForeignKey(
                name: "fk_alerts_sensor_id",
                table: "alerts");

            migrationBuilder.DropForeignKey(
                name: "fk_alerts_site_id",
                table: "alerts");

            migrationBuilder.DropIndex(
                name: "IX_alerts_equipment_id",
                table: "alerts");

            migrationBuilder.DropIndex(
                name: "ix_alerts_organization_id",
                table: "alerts");

            migrationBuilder.DropIndex(
                name: "ix_alerts_sensor_id",
                table: "alerts");

            migrationBuilder.DropIndex(
                name: "ix_alerts_sensor_id_status",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "equipment_id",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "resolved_at",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "sensor_id",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "severity",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "threshold_max",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "threshold_min",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "triggered_at",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "triggered_value",
                table: "alerts");

            migrationBuilder.RenameColumn(
                name: "site_id",
                table: "alerts",
                newName: "sensor_reading_id");

            migrationBuilder.RenameIndex(
                name: "IX_alerts_site_id",
                table: "alerts",
                newName: "ix_alerts_sensor_reading_id");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "alerts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "alerts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "alerts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "alerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_sensor_reading_id",
                table: "alerts",
                column: "sensor_reading_id",
                principalTable: "sensor_readings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
