using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgpPeriodEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMilestone",
                table: "AgpTimelineMilestones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PeriodName",
                table: "AgpPeriods",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HoursPerWeek",
                table: "AgpActivities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "AgpActivities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "NeedsReview",
                table: "AgpActivities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "AgpActivities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "AgpActivities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AgpActivities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMilestone",
                table: "AgpTimelineMilestones");

            migrationBuilder.DropColumn(
                name: "PeriodName",
                table: "AgpPeriods");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "AgpActivities");

            migrationBuilder.DropColumn(
                name: "NeedsReview",
                table: "AgpActivities");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "AgpActivities");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "AgpActivities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AgpActivities");

            migrationBuilder.AlterColumn<int>(
                name: "HoursPerWeek",
                table: "AgpActivities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
