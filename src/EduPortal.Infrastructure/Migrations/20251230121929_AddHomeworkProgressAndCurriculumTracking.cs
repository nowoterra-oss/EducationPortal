using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkProgressAndCurriculumTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseResources_Curricula_CurriculumId",
                table: "CourseResources");

            migrationBuilder.DropForeignKey(
                name: "FK_Curricula_Courses_CourseId",
                table: "Curricula");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Curricula",
                table: "Curricula");

            migrationBuilder.RenameTable(
                name: "Curricula",
                newName: "Curriculum");

            migrationBuilder.RenameIndex(
                name: "IX_Curricula_CourseId",
                table: "Curriculum",
                newName: "IX_Curriculum_CourseId");

            migrationBuilder.AddColumn<int>(
                name: "CurriculumId",
                table: "HomeworkAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercentage",
                table: "HomeworkAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "HomeworkAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEvaluationCompleted",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LessonEvaluation",
                table: "Attendances",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Performance",
                table: "Attendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "Curriculum",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExamResourceId",
                table: "Curriculum",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasExam",
                table: "Curriculum",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Curriculum",
                table: "Curriculum",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "HomeworkAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeworkAssignmentId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    IsFromCourseResource = table.Column<bool>(type: "bit", nullable: false),
                    CourseResourceId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkAttachments_CourseResources_CourseResourceId",
                        column: x => x.CourseResourceId,
                        principalTable: "CourseResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HomeworkAttachments_HomeworkAssignments_HomeworkAssignmentId",
                        column: x => x.HomeworkAssignmentId,
                        principalTable: "HomeworkAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCurriculumProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CurriculumId = table.Column<int>(type: "int", nullable: false),
                    IsTopicCompleted = table.Column<bool>(type: "bit", nullable: false),
                    TopicCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AreHomeworksCompleted = table.Column<bool>(type: "bit", nullable: false),
                    HomeworksCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExamUnlocked = table.Column<bool>(type: "bit", nullable: false),
                    ExamUnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExamCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ExamCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamScore = table.Column<int>(type: "int", nullable: true),
                    IsApprovedByTeacher = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedByTeacherId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCurriculumProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCurriculumProgresses_Curriculum_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curriculum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCurriculumProgresses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCurriculumProgresses_Teachers_ApprovedByTeacherId",
                        column: x => x.ApprovedByTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_CurriculumId",
                table: "HomeworkAssignments",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculum_CourseId1",
                table: "Curriculum",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculum_ExamResourceId",
                table: "Curriculum",
                column: "ExamResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAttachments_CourseResourceId",
                table: "HomeworkAttachments",
                column: "CourseResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAttachments_HomeworkAssignmentId",
                table: "HomeworkAttachments",
                column: "HomeworkAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCurriculumProgresses_ApprovedByTeacherId",
                table: "StudentCurriculumProgresses",
                column: "ApprovedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCurriculumProgresses_CurriculumId",
                table: "StudentCurriculumProgresses",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCurriculumProgresses_StudentId_CurriculumId",
                table: "StudentCurriculumProgresses",
                columns: new[] { "StudentId", "CurriculumId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseResources_Curriculum_CurriculumId",
                table: "CourseResources",
                column: "CurriculumId",
                principalTable: "Curriculum",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Curriculum_CourseResources_ExamResourceId",
                table: "Curriculum",
                column: "ExamResourceId",
                principalTable: "CourseResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Curriculum_Courses_CourseId",
                table: "Curriculum",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Curriculum_Courses_CourseId1",
                table: "Curriculum",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeworkAssignments_Curriculum_CurriculumId",
                table: "HomeworkAssignments",
                column: "CurriculumId",
                principalTable: "Curriculum",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseResources_Curriculum_CurriculumId",
                table: "CourseResources");

            migrationBuilder.DropForeignKey(
                name: "FK_Curriculum_CourseResources_ExamResourceId",
                table: "Curriculum");

            migrationBuilder.DropForeignKey(
                name: "FK_Curriculum_Courses_CourseId",
                table: "Curriculum");

            migrationBuilder.DropForeignKey(
                name: "FK_Curriculum_Courses_CourseId1",
                table: "Curriculum");

            migrationBuilder.DropForeignKey(
                name: "FK_HomeworkAssignments_Curriculum_CurriculumId",
                table: "HomeworkAssignments");

            migrationBuilder.DropTable(
                name: "HomeworkAttachments");

            migrationBuilder.DropTable(
                name: "StudentCurriculumProgresses");

            migrationBuilder.DropIndex(
                name: "IX_HomeworkAssignments_CurriculumId",
                table: "HomeworkAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Curriculum",
                table: "Curriculum");

            migrationBuilder.DropIndex(
                name: "IX_Curriculum_CourseId1",
                table: "Curriculum");

            migrationBuilder.DropIndex(
                name: "IX_Curriculum_ExamResourceId",
                table: "Curriculum");

            migrationBuilder.DropColumn(
                name: "CurriculumId",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "HomeworkAssignments");

            migrationBuilder.DropColumn(
                name: "IsEvaluationCompleted",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "LessonEvaluation",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Performance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "Curriculum");

            migrationBuilder.DropColumn(
                name: "ExamResourceId",
                table: "Curriculum");

            migrationBuilder.DropColumn(
                name: "HasExam",
                table: "Curriculum");

            migrationBuilder.RenameTable(
                name: "Curriculum",
                newName: "Curricula");

            migrationBuilder.RenameIndex(
                name: "IX_Curriculum_CourseId",
                table: "Curricula",
                newName: "IX_Curricula_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Curricula",
                table: "Curricula",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseResources_Curricula_CurriculumId",
                table: "CourseResources",
                column: "CurriculumId",
                principalTable: "Curricula",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Curricula_Courses_CourseId",
                table: "Curricula",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
