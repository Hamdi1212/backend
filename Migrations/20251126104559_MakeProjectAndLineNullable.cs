using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Checklist.Migrations
{
    /// <inheritdoc />
    public partial class MakeProjectAndLineNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "LineId",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LineId",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
