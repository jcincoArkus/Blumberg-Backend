using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIngestionRunFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "accepted_records",
                table: "ingestion_runs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at",
                table: "ingestion_runs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "ingestion_runs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "rejected_records",
                table: "ingestion_runs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "ingestion_runs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "started_at",
                table: "ingestion_runs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "ingestion_runs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "total_records",
                table: "ingestion_runs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_ingestion_runs_organization_id",
                table: "ingestion_runs",
                column: "organization_id");

            migrationBuilder.AddForeignKey(
                name: "fk_ingestion_runs_organization_id",
                table: "ingestion_runs",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ingestion_runs_organization_id",
                table: "ingestion_runs");

            migrationBuilder.DropIndex(
                name: "ix_ingestion_runs_organization_id",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "accepted_records",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "rejected_records",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "source",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "status",
                table: "ingestion_runs");

            migrationBuilder.DropColumn(
                name: "total_records",
                table: "ingestion_runs");
        }
    }
}
