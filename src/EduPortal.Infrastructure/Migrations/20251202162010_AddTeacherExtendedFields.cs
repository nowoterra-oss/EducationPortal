using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "Teachers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Certifications",
                table: "Teachers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Teachers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "Teachers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Teachers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeHours",
                table: "Teachers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeLocation",
                table: "Teachers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Biography",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Certifications",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "OfficeHours",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "OfficeLocation",
                table: "Teachers");
        }
    }
}
