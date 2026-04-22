using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class SensorHasThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "threshold_id",
                table: "sensors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_sensors_threshold_id_unique",
                table: "sensors",
                column: "threshold_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_sensors_threshold_id",
                table: "sensors",
                column: "threshold_id",
                principalTable: "thresholds",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sensors_threshold_id",
                table: "sensors");

            migrationBuilder.DropIndex(
                name: "ix_sensors_threshold_id_unique",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "threshold_id",
                table: "sensors");
        }
    }
}
