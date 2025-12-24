using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeCourseIdNullableInStudentTeacherAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentTeacherAssignments_Courses_CourseId",
                table: "StudentTeacherAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "StudentTeacherAssignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTeacherAssignments_Courses_CourseId",
                table: "StudentTeacherAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentTeacherAssignments_Courses_CourseId",
                table: "StudentTeacherAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "StudentTeacherAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTeacherAssignments_Courses_CourseId",
                table: "StudentTeacherAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
