using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class OrgHasSites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "sites",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_sites_organization_id",
                table: "sites",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "fk_sites_organization_id",
                table: "sites",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sites_organization_id",
                table: "sites");

            migrationBuilder.DropIndex(
                name: "ix_sites_organization_id",
                table: "sites");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "sites");
        }
    }
}
