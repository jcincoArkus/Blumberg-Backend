using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class SiteHasEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "site_id",
                table: "equipment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_equipment_site_id",
                table: "equipment",
                column: "site_id");

            migrationBuilder.AddForeignKey(
                name: "fk_equipment_site_id",
                table: "equipment",
                column: "site_id",
                principalTable: "sites",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_equipment_site_id",
                table: "equipment");

            migrationBuilder.DropIndex(
                name: "ix_equipment_site_id",
                table: "equipment");

            migrationBuilder.DropColumn(
                name: "site_id",
                table: "equipment");
        }
    }
}
