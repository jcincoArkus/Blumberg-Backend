using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class OrgHasAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_admins_email_unique",
                table: "admins");

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "admins",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_admins_organization_id",
                table: "admins",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_admins_organization_id_email_unique",
                table: "admins",
                columns: new[] { "organization_id", "email" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_admins_organization_id",
                table: "admins",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_admins_organization_id",
                table: "admins");

            migrationBuilder.DropIndex(
                name: "ix_admins_organization_id",
                table: "admins");

            migrationBuilder.DropIndex(
                name: "ix_admins_organization_id_email_unique",
                table: "admins");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "admins");

            migrationBuilder.CreateIndex(
                name: "ix_admins_email_unique",
                table: "admins",
                column: "email",
                unique: true);
        }
    }
}
