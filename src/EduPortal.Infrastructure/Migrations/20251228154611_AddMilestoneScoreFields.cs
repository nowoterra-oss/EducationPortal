using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMilestoneScoreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "AgpTimelineMilestones",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxScore",
                table: "AgpTimelineMilestones",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultNotes",
                table: "AgpTimelineMilestones",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "AgpTimelineMilestones",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "AgpTimelineMilestones");

            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "AgpTimelineMilestones");

            migrationBuilder.DropColumn(
                name: "ResultNotes",
                table: "AgpTimelineMilestones");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "AgpTimelineMilestones");
        }
    }
}
