using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCounselorDashboardEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CharacterixResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssessmentVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResultsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Analysis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Interpretation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReportUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterixResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterixResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounselorNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CounselorId = table.Column<int>(type: "int", nullable: false),
                    CounselingMeetingId = table.Column<int>(type: "int", nullable: true),
                    NoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NoteContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedTasks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextMeetingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SendEmailToParent = table.Column<bool>(type: "bit", nullable: false),
                    SendSmsToParent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    SmsSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SmsSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounselorNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounselorNotes_CounselingMeetings_CounselingMeetingId",
                        column: x => x.CounselingMeetingId,
                        principalTable: "CounselingMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CounselorNotes_Counselors_CounselorId",
                        column: x => x.CounselorId,
                        principalTable: "Counselors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CounselorNotes_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentAwards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    AwardName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssuingOrganization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AwardDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAwards_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentExamCalendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ExamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistrationStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetScore = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ActualScore = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ReminderSent7Days = table.Column<bool>(type: "bit", nullable: false),
                    ReminderSent1Day = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentExamCalendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentExamCalendars_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniversityRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniversityApplicationId = table.Column<int>(type: "int", nullable: false),
                    RequirementName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequirementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversityRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversityRequirements_UniversityApplications_UniversityApplicationId",
                        column: x => x.UniversityApplicationId,
                        principalTable: "UniversityApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterixResults_StudentId",
                table: "CharacterixResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselorNotes_CounselingMeetingId",
                table: "CounselorNotes",
                column: "CounselingMeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselorNotes_CounselorId",
                table: "CounselorNotes",
                column: "CounselorId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselorNotes_NoteDate",
                table: "CounselorNotes",
                column: "NoteDate");

            migrationBuilder.CreateIndex(
                name: "IX_CounselorNotes_StudentId",
                table: "CounselorNotes",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAwards_Category",
                table: "StudentAwards",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAwards_Scope",
                table: "StudentAwards",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAwards_StudentId",
                table: "StudentAwards",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamCalendars_ExamDate",
                table: "StudentExamCalendars",
                column: "ExamDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamCalendars_ExamType",
                table: "StudentExamCalendars",
                column: "ExamType");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamCalendars_StudentId",
                table: "StudentExamCalendars",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversityRequirements_UniversityApplicationId",
                table: "UniversityRequirements",
                column: "UniversityApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterixResults");

            migrationBuilder.DropTable(
                name: "CounselorNotes");

            migrationBuilder.DropTable(
                name: "StudentAwards");

            migrationBuilder.DropTable(
                name: "StudentExamCalendars");

            migrationBuilder.DropTable(
                name: "UniversityRequirements");
        }
    }
}
