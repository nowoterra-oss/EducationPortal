using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationDatesToMilestone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationEndDate",
                table: "AgpTimelineMilestones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationStartDate",
                table: "AgpTimelineMilestones",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationEndDate",
                table: "AgpTimelineMilestones");

            migrationBuilder.DropColumn(
                name: "ApplicationStartDate",
                table: "AgpTimelineMilestones");
        }
    }
}
