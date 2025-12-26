using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCoachingSystemAddTeacherCounselor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CareerAssessments_Coaches_CoachId",
                table: "CareerAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CoachingSessions_CoachingSessionId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolRecommendations_Coaches_CoachId",
                table: "SchoolRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_SportsAssessments_Coaches_CoachId",
                table: "SportsAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyAbroadPrograms_Coaches_CoachId",
                table: "StudyAbroadPrograms");

            migrationBuilder.DropTable(
                name: "CoachingSessions");

            migrationBuilder.DropTable(
                name: "StudentCoachAssignments");

            migrationBuilder.DropTable(
                name: "Coaches");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CoachingSessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CoachingSessionId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "Teachers",
                newName: "CounselorId");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "StudyAbroadPrograms",
                newName: "CounselorId");

            migrationBuilder.RenameIndex(
                name: "IX_StudyAbroadPrograms_CoachId",
                table: "StudyAbroadPrograms",
                newName: "IX_StudyAbroadPrograms_CounselorId");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "SportsAssessments",
                newName: "CounselorId");

            migrationBuilder.RenameIndex(
                name: "IX_SportsAssessments_CoachId",
                table: "SportsAssessments",
                newName: "IX_SportsAssessments_CounselorId");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "SchoolRecommendations",
                newName: "CounselorId");

            migrationBuilder.RenameIndex(
                name: "IX_SchoolRecommendations_CoachId",
                table: "SchoolRecommendations",
                newName: "IX_SchoolRecommendations_CounselorId");

            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "CareerAssessments",
                newName: "CounselorId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerAssessments_CoachId",
                table: "CareerAssessments",
                newName: "IX_CareerAssessments_CounselorId");

            migrationBuilder.AddColumn<bool>(
                name: "IsAlsoCounselor",
                table: "Teachers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Counselors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_CounselorId",
                table: "Teachers",
                column: "CounselorId");

            migrationBuilder.CreateIndex(
                name: "IX_Counselors_TeacherId",
                table: "Counselors",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_CareerAssessments_Counselors_CounselorId",
                table: "CareerAssessments",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Counselors_Teachers_TeacherId",
                table: "Counselors",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolRecommendations_Counselors_CounselorId",
                table: "SchoolRecommendations",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SportsAssessments_Counselors_CounselorId",
                table: "SportsAssessments",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyAbroadPrograms_Counselors_CounselorId",
                table: "StudyAbroadPrograms",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Counselors_CounselorId",
                table: "Teachers",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CareerAssessments_Counselors_CounselorId",
                table: "CareerAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Counselors_Teachers_TeacherId",
                table: "Counselors");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolRecommendations_Counselors_CounselorId",
                table: "SchoolRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_SportsAssessments_Counselors_CounselorId",
                table: "SportsAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyAbroadPrograms_Counselors_CounselorId",
                table: "StudyAbroadPrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Counselors_CounselorId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_CounselorId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Counselors_TeacherId",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "IsAlsoCounselor",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Counselors");

            migrationBuilder.RenameColumn(
                name: "CounselorId",
                table: "Teachers",
                newName: "CoachId");

            migrationBuilder.RenameColumn(
                name: "CounselorId",
                table: "StudyAbroadPrograms",
                newName: "CoachId");

            migrationBuilder.RenameIndex(
                name: "IX_StudyAbroadPrograms_CounselorId",
                table: "StudyAbroadPrograms",
                newName: "IX_StudyAbroadPrograms_CoachId");

            migrationBuilder.RenameColumn(
                name: "CounselorId",
                table: "SportsAssessments",
                newName: "CoachId");

            migrationBuilder.RenameIndex(
                name: "IX_SportsAssessments_CounselorId",
                table: "SportsAssessments",
                newName: "IX_SportsAssessments_CoachId");

            migrationBuilder.RenameColumn(
                name: "CounselorId",
                table: "SchoolRecommendations",
                newName: "CoachId");

            migrationBuilder.RenameIndex(
                name: "IX_SchoolRecommendations_CounselorId",
                table: "SchoolRecommendations",
                newName: "IX_SchoolRecommendations_CoachId");

            migrationBuilder.RenameColumn(
                name: "CounselorId",
                table: "CareerAssessments",
                newName: "CoachId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerAssessments_CounselorId",
                table: "CareerAssessments",
                newName: "IX_CareerAssessments_CoachId");

            migrationBuilder.AddColumn<int>(
                name: "CoachingSessionId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    TeacherId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AreasJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsAlsoTeacher = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Qualifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coaches_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Coaches_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CoachingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    CoachId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ActionItems = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoachingArea = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NextSessionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionNotes = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    SessionType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StudentFeedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachingSessions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CoachingSessions_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachingSessions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCoachAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CoachingArea = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Goals = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCoachAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCoachAssignments_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCoachAssignments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CoachingSessionId",
                table: "Payments",
                column: "CoachingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_BranchId",
                table: "Coaches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_TeacherId",
                table: "Coaches",
                column: "TeacherId",
                unique: true,
                filter: "[TeacherId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_UserId",
                table: "Coaches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachingSessions_BranchId",
                table: "CoachingSessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachingSessions_CoachId",
                table: "CoachingSessions",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachingSessions_StudentId",
                table: "CoachingSessions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCoachAssignments_CoachId",
                table: "StudentCoachAssignments",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCoachAssignments_StudentId",
                table: "StudentCoachAssignments",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CareerAssessments_Coaches_CoachId",
                table: "CareerAssessments",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CoachingSessions_CoachingSessionId",
                table: "Payments",
                column: "CoachingSessionId",
                principalTable: "CoachingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolRecommendations_Coaches_CoachId",
                table: "SchoolRecommendations",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SportsAssessments_Coaches_CoachId",
                table: "SportsAssessments",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyAbroadPrograms_Coaches_CoachId",
                table: "StudyAbroadPrograms",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
