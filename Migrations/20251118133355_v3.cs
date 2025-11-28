using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Checklist.Migrations
{
    /// <inheritdoc />
    public partial class v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Checklists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotificationStatus",
                table: "Checklists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Shift",
                table: "Checklists",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "NotificationStatus",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "Shift",
                table: "Checklists");
        }
    }
}
