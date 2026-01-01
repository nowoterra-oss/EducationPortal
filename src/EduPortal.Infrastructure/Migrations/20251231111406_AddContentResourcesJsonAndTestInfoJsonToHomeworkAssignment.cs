using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentResourcesJsonAndTestInfoJsonToHomeworkAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentResourcesJson",
                table: "HomeworkAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TestDueDate",
                table: "HomeworkAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestInfoJson",
                table: "HomeworkAssignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentResourcesJson",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TestDueDate",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "TestInfoJson",
                table: "HomeworkAssignments");
        }
    }
}
