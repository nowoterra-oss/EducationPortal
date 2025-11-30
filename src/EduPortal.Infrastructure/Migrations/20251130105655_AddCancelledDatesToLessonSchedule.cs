using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledDatesToLessonSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelledDates",
                table: "LessonSchedules",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledDates",
                table: "LessonSchedules");
        }
    }
}
