using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class OrgHasSensorReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "sensor_readings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_organization_id",
                table: "sensor_readings",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "fk_sensor_readings_organization_id",
                table: "sensor_readings",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sensor_readings_organization_id",
                table: "sensor_readings");

            migrationBuilder.DropIndex(
                name: "ix_sensor_readings_organization_id",
                table: "sensor_readings");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "sensor_readings");
        }
    }
}
