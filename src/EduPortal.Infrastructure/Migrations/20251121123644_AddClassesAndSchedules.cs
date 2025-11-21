using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassesAndSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicDevelopmentPlans_Students_StudentId",
                table: "AcademicDevelopmentPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Students_StudentId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Teachers_TeacherId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassPerformances_Courses_CourseId",
                table: "ClassPerformances");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassPerformances_Students_StudentId",
                table: "ClassPerformances");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassPerformances_Teachers_TeacherId",
                table: "ClassPerformances");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionsAndAwards_Students_StudentId",
                table: "CompetitionsAndAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_CounselingMeetings_Counselors_CounselorId",
                table: "CounselingMeetings");

            migrationBuilder.DropForeignKey(
                name: "FK_CounselingMeetings_Students_StudentId",
                table: "CounselingMeetings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Students_StudentId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Homeworks_Courses_CourseId",
                table: "Homeworks");

            migrationBuilder.DropForeignKey(
                name: "FK_Homeworks_Teachers_TeacherId",
                table: "Homeworks");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalExams_Courses_CourseId",
                table: "InternalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalExams_Teachers_TeacherId",
                table: "InternalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_InternationalExams_Students_StudentId",
                table: "InternationalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Students_StudentId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentPlans_Students_StudentId",
                table: "PaymentPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCounselorAssignments_Counselors_CounselorId",
                table: "StudentCounselorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCounselorAssignments_Students_StudentId",
                table: "StudentCounselorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentDocuments_Students_StudentId",
                table: "StudentDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentHomeworkSubmissions_Homeworks_HomeworkId",
                table: "StudentHomeworkSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentHomeworkSubmissions_Students_StudentId",
                table: "StudentHomeworkSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTeacherAssignments_Students_StudentId",
                table: "StudentTeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTeacherAssignments_Teachers_TeacherId",
                table: "StudentTeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UniversityApplications_Students_StudentId",
                table: "UniversityApplications");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Parents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TermName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TermNumber = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MidtermStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MidtermEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetAudience = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_AspNetUsers_PublishedBy",
                        column: x => x.PublishedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClassTeacherId = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Teachers_ClassTeacherId",
                        column: x => x.ClassTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuildingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Equipment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasProjector = table.Column<bool>(type: "bit", nullable: false),
                    HasSmartBoard = table.Column<bool>(type: "bit", nullable: false),
                    HasComputer = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsLab = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AdvisorTeacherId = table.Column<int>(type: "int", nullable: true),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    CurrentMembers = table.Column<int>(type: "int", nullable: false),
                    MeetingDay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MeetingTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MeetingRoom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AcceptingMembers = table.Column<bool>(type: "bit", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clubs_Teachers_AdvisorTeacherId",
                        column: x => x.AdvisorTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    InstallmentId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentInstallments_InstallmentId",
                        column: x => x.InstallmentId,
                        principalTable: "PaymentInstallments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentClassAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClassAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentClassAssignments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentClassAssignments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklySchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    ClassroomId = table.Column<int>(type: "int", nullable: true),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parents_ApplicationUserId",
                table: "Parents",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_PublishedBy",
                table: "Announcements",
                column: "PublishedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ClassTeacherId",
                table: "Classes",
                column: "ClassTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_AdvisorTeacherId",
                table: "Clubs",
                column: "AdvisorTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InstallmentId",
                table: "Payments",
                column: "InstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassAssignments_ClassId",
                table: "StudentClassAssignments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassAssignments_StudentId",
                table: "StudentClassAssignments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_ClassId",
                table: "WeeklySchedules",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_ClassroomId",
                table: "WeeklySchedules",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_CourseId",
                table: "WeeklySchedules",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_TeacherId",
                table: "WeeklySchedules",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicDevelopmentPlans_Students_StudentId",
                table: "AcademicDevelopmentPlans",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Students_StudentId",
                table: "Attendances",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Teachers_TeacherId",
                table: "Attendances",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassPerformances_Courses_CourseId",
                table: "ClassPerformances",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassPerformances_Students_StudentId",
                table: "ClassPerformances",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassPerformances_Teachers_TeacherId",
                table: "ClassPerformances",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionsAndAwards_Students_StudentId",
                table: "CompetitionsAndAwards",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CounselingMeetings_Counselors_CounselorId",
                table: "CounselingMeetings",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CounselingMeetings_Students_StudentId",
                table: "CounselingMeetings",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Students_StudentId",
                table: "ExamResults",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Homeworks_Courses_CourseId",
                table: "Homeworks",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Homeworks_Teachers_TeacherId",
                table: "Homeworks",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalExams_Courses_CourseId",
                table: "InternalExams",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalExams_Teachers_TeacherId",
                table: "InternalExams",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternationalExams_Students_StudentId",
                table: "InternationalExams",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_AspNetUsers_ApplicationUserId",
                table: "Parents",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Students_StudentId",
                table: "Parents",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentPlans_Students_StudentId",
                table: "PaymentPlans",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCounselorAssignments_Counselors_CounselorId",
                table: "StudentCounselorAssignments",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCounselorAssignments_Students_StudentId",
                table: "StudentCounselorAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentDocuments_Students_StudentId",
                table: "StudentDocuments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentHomeworkSubmissions_Homeworks_HomeworkId",
                table: "StudentHomeworkSubmissions",
                column: "HomeworkId",
                principalTable: "Homeworks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentHomeworkSubmissions_Students_StudentId",
                table: "StudentHomeworkSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTeacherAssignments_Students_StudentId",
                table: "StudentTeacherAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTeacherAssignments_Teachers_TeacherId",
                table: "StudentTeacherAssignments",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UniversityApplications_Students_StudentId",
                table: "UniversityApplications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicDevelopmentPlans_Students_StudentId",
                table: "AcademicDevelopmentPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Students_StudentId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Teachers_TeacherId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassPerformances_Courses_CourseId",
                table: "ClassPerformances");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassPerformances_Students_StudentId",
                table: "ClassPerformances");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassPerformances_Teachers_TeacherId",
                table: "ClassPerformances");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionsAndAwards_Students_StudentId",
                table: "CompetitionsAndAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_CounselingMeetings_Counselors_CounselorId",
                table: "CounselingMeetings");

            migrationBuilder.DropForeignKey(
                name: "FK_CounselingMeetings_Students_StudentId",
                table: "CounselingMeetings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResults_Students_StudentId",
                table: "ExamResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Homeworks_Courses_CourseId",
                table: "Homeworks");

            migrationBuilder.DropForeignKey(
                name: "FK_Homeworks_Teachers_TeacherId",
                table: "Homeworks");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalExams_Courses_CourseId",
                table: "InternalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalExams_Teachers_TeacherId",
                table: "InternalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_InternationalExams_Students_StudentId",
                table: "InternationalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_AspNetUsers_ApplicationUserId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Students_StudentId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentPlans_Students_StudentId",
                table: "PaymentPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCounselorAssignments_Counselors_CounselorId",
                table: "StudentCounselorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCounselorAssignments_Students_StudentId",
                table: "StudentCounselorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentDocuments_Students_StudentId",
                table: "StudentDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentHomeworkSubmissions_Homeworks_HomeworkId",
                table: "StudentHomeworkSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentHomeworkSubmissions_Students_StudentId",
                table: "StudentHomeworkSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTeacherAssignments_Students_StudentId",
                table: "StudentTeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTeacherAssignments_Teachers_TeacherId",
                table: "StudentTeacherAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UniversityApplications_Students_StudentId",
                table: "UniversityApplications");

            migrationBuilder.DropTable(
                name: "AcademicTerms");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "Clubs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "StudentClassAssignments");

            migrationBuilder.DropTable(
                name: "WeeklySchedules");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropIndex(
                name: "IX_Parents_ApplicationUserId",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Parents");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicDevelopmentPlans_Students_StudentId",
                table: "AcademicDevelopmentPlans",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Courses_CourseId",
                table: "Attendances",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Students_StudentId",
                table: "Attendances",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Teachers_TeacherId",
                table: "Attendances",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassPerformances_Courses_CourseId",
                table: "ClassPerformances",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassPerformances_Students_StudentId",
                table: "ClassPerformances",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassPerformances_Teachers_TeacherId",
                table: "ClassPerformances",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionsAndAwards_Students_StudentId",
                table: "CompetitionsAndAwards",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CounselingMeetings_Counselors_CounselorId",
                table: "CounselingMeetings",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CounselingMeetings_Students_StudentId",
                table: "CounselingMeetings",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResults_Students_StudentId",
                table: "ExamResults",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Homeworks_Courses_CourseId",
                table: "Homeworks",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Homeworks_Teachers_TeacherId",
                table: "Homeworks",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalExams_Courses_CourseId",
                table: "InternalExams",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalExams_Teachers_TeacherId",
                table: "InternalExams",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternationalExams_Students_StudentId",
                table: "InternationalExams",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Students_StudentId",
                table: "Parents",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentPlans_Students_StudentId",
                table: "PaymentPlans",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCounselorAssignments_Counselors_CounselorId",
                table: "StudentCounselorAssignments",
                column: "CounselorId",
                principalTable: "Counselors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCounselorAssignments_Students_StudentId",
                table: "StudentCounselorAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentDocuments_Students_StudentId",
                table: "StudentDocuments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentHomeworkSubmissions_Homeworks_HomeworkId",
                table: "StudentHomeworkSubmissions",
                column: "HomeworkId",
                principalTable: "Homeworks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentHomeworkSubmissions_Students_StudentId",
                table: "StudentHomeworkSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTeacherAssignments_Students_StudentId",
                table: "StudentTeacherAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTeacherAssignments_Teachers_TeacherId",
                table: "StudentTeacherAssignments",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UniversityApplications_Students_StudentId",
                table: "UniversityApplications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
