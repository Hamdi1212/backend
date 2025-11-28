using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Checklist.Migrations
{
    public partial class AddTemplateChecklistForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, clean up any orphaned data
            migrationBuilder.Sql(@"
                DELETE FROM Checklists 
                WHERE TemplateId NOT IN (SELECT Id FROM Templates)
            ");

            // Create index
            migrationBuilder.CreateIndex(
                name: "IX_Checklists_TemplateId",
                table: "Checklists",
                column: "TemplateId");

            // Add foreign key with Restrict to prevent template deletion if in use
            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists");

            migrationBuilder.DropIndex(
                name: "IX_Checklists_TemplateId",
                table: "Checklists");
        }
    }
}