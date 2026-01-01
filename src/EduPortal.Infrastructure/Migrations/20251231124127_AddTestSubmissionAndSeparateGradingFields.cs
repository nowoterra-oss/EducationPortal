using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestSubmissionAndSeparateGradingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeworkFeedback",
                table: "HomeworkAssignments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeworkScore",
                table: "HomeworkAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestFeedback",
                table: "HomeworkAssignments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestScore",
                table: "HomeworkAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestSubmissionText",
                table: "HomeworkAssignments",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestSubmissionUrl",
                table: "HomeworkAssignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TestSubmittedAt",
                table: "HomeworkAssignments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeworkFeedback",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "HomeworkScore",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TestFeedback",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TestScore",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TestSubmissionText",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TestSubmissionUrl",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TestSubmittedAt",
                table: "HomeworkAssignments");
        }
    }
}
