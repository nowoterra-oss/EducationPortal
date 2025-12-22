using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSimpleInternshipAndCertificateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateFileName",
                table: "StudentHobbies",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateUrl",
                table: "StudentHobbies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StudentHobbies",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "StudentHobbies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateFileName",
                table: "StudentActivities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateUrl",
                table: "StudentActivities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SimpleInternships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOngoing = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CertificateFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleInternships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimpleInternships_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SimpleInternships_StudentId",
                table: "SimpleInternships",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimpleInternships");

            migrationBuilder.DropColumn(
                name: "CertificateFileName",
                table: "StudentHobbies");

            migrationBuilder.DropColumn(
                name: "CertificateUrl",
                table: "StudentHobbies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StudentHobbies");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "StudentHobbies");

            migrationBuilder.DropColumn(
                name: "CertificateFileName",
                table: "StudentActivities");

            migrationBuilder.DropColumn(
                name: "CertificateUrl",
                table: "StudentActivities");
        }
    }
}
