using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityFieldsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IdentityType",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IdentityType",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Students");
        }
    }
}
