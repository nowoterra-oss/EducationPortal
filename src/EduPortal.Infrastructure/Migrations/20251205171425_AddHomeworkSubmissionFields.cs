using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkSubmissionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeworkSubmissionFiles_StudentHomeworkSubmissions_SubmissionId",
                table: "HomeworkSubmissionFiles");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "HomeworkSubmissionFiles");

            migrationBuilder.RenameColumn(
                name: "SubmissionId",
                table: "HomeworkSubmissionFiles",
                newName: "HomeworkAssignmentId");

            migrationBuilder.RenameColumn(
                name: "FileSizeBytes",
                table: "HomeworkSubmissionFiles",
                newName: "FileSize");

            migrationBuilder.RenameIndex(
                name: "IX_HomeworkSubmissionFiles_SubmissionId",
                table: "HomeworkSubmissionFiles",
                newName: "IX_HomeworkSubmissionFiles_HomeworkAssignmentId");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "HomeworkSubmissionFiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "HomeworkSubmissionFiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SubmissionText",
                table: "HomeworkAssignments",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionUrl",
                table: "HomeworkAssignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "HomeworkAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HomeworkSubmissionFiles_HomeworkAssignments_HomeworkAssignmentId",
                table: "HomeworkSubmissionFiles",
                column: "HomeworkAssignmentId",
                principalTable: "HomeworkAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeworkSubmissionFiles_HomeworkAssignments_HomeworkAssignmentId",
                table: "HomeworkSubmissionFiles");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "HomeworkSubmissionFiles");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "HomeworkSubmissionFiles");

            migrationBuilder.DropColumn(
                name: "SubmissionText",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "SubmissionUrl",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "HomeworkAssignments");

            migrationBuilder.RenameColumn(
                name: "HomeworkAssignmentId",
                table: "HomeworkSubmissionFiles",
                newName: "SubmissionId");

            migrationBuilder.RenameColumn(
                name: "FileSize",
                table: "HomeworkSubmissionFiles",
                newName: "FileSizeBytes");

            migrationBuilder.RenameIndex(
                name: "IX_HomeworkSubmissionFiles_HomeworkAssignmentId",
                table: "HomeworkSubmissionFiles",
                newName: "IX_HomeworkSubmissionFiles_SubmissionId");

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "HomeworkSubmissionFiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeworkSubmissionFiles_StudentHomeworkSubmissions_SubmissionId",
                table: "HomeworkSubmissionFiles",
                column: "SubmissionId",
                principalTable: "StudentHomeworkSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
