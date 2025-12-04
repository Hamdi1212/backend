using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Checklist.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipsClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Users_UserId",
                table: "Checklists");

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId1",
                table: "Questions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LineId1",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId1",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId1",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Checklists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TemplateId1",
                table: "Questions",
                column: "TemplateId1");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_LineId1",
                table: "Checklists",
                column: "LineId1");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_ProjectId1",
                table: "Checklists",
                column: "ProjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_TemplateId1",
                table: "Checklists",
                column: "TemplateId1");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_UserId1",
                table: "Checklists",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Lines_LineId1",
                table: "Checklists",
                column: "LineId1",
                principalTable: "Lines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Projects_ProjectId1",
                table: "Checklists",
                column: "ProjectId1",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Templates_TemplateId1",
                table: "Checklists",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Users_UserId",
                table: "Checklists",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Users_UserId1",
                table: "Checklists",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Templates_TemplateId1",
                table: "Questions",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Lines_LineId1",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Projects_ProjectId1",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Templates_TemplateId1",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Users_UserId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Users_UserId1",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Templates_TemplateId1",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_TemplateId1",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Checklists_LineId1",
                table: "Checklists");

            migrationBuilder.DropIndex(
                name: "IX_Checklists_ProjectId1",
                table: "Checklists");

            migrationBuilder.DropIndex(
                name: "IX_Checklists_TemplateId1",
                table: "Checklists");

            migrationBuilder.DropIndex(
                name: "IX_Checklists_UserId1",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "TemplateId1",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "LineId1",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "ProjectId1",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "TemplateId1",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Checklists");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Users_UserId",
                table: "Checklists",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
