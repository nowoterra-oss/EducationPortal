using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentActivityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "StudentActivities",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "Achievements",
                table: "StudentActivities",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOngoing",
                table: "StudentActivities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "StudentActivities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Achievements",
                table: "StudentActivities");

            migrationBuilder.DropColumn(
                name: "IsOngoing",
                table: "StudentActivities");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "StudentActivities");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "StudentActivities",
                newName: "Category");
        }
    }
}
