using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class ThresholdTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "thresholds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    min = table.Column<decimal>(type: "numeric", nullable: false),
                    max = table.Column<decimal>(type: "numeric", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thresholds_id", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_thresholds_created_at_desc",
                table: "thresholds",
                column: "created_at",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "thresholds");
        }
    }
}
