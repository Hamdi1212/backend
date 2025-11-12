using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Checklist.Migrations
{
    /// <inheritdoc />
    public partial class Title : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProjects",
                table: "UserProjects");

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
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");

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

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Checklists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProjects",
                table: "UserProjects",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UserId",
                table: "UserProjects",
                column: "UserId");

            // Keep cascade where it is safe, but avoid SQL Server multiple cascade paths
            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Lines_LineId",
                table: "Checklists",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Changed to NoAction to avoid multiple cascade paths
            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Projects_ProjectId",
                table: "Checklists",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            // Changed to NoAction to avoid multiple cascade paths
            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Users_UserId",
                table: "Checklists",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Templates_TemplateId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Users_UserId",
                table: "Checklists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProjects",
                table: "UserProjects");

            migrationBuilder.DropIndex(
                name: "IX_UserProjects_UserId",
                table: "UserProjects");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Checklists");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "datetime2",
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

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProjects",
                table: "UserProjects",
                columns: new[] { "UserId", "ProjectId" });

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
        }
    }
}
