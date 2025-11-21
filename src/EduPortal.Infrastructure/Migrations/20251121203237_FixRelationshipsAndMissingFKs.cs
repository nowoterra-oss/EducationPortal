using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipsAndMissingFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Students_StudentId",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Parents_StudentId",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "IsParentsSeparated",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "ParentType",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Parents");

            migrationBuilder.AddColumn<int>(
                name: "AcademicTermId",
                table: "WeeklySchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AcademicTermId",
                table: "StudentClassAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentInstallmentId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentPlanId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPhone",
                table: "Parents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicTermId",
                table: "InternalExams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "InternalExams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicTermId",
                table: "Homeworks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "Homeworks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "CalendarEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "CalendarEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StudentParents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimaryContact = table.Column<bool>(type: "bit", nullable: false),
                    IsEmergencyContact = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentParents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentParents_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentParents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_AcademicTermId",
                table: "WeeklySchedules",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassAssignments_AcademicTermId",
                table: "StudentClassAssignments",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentInstallmentId",
                table: "Payments",
                column: "PaymentInstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentPlanId",
                table: "Payments",
                column: "PaymentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalExams_AcademicTermId",
                table: "InternalExams",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalExams_ClassId",
                table: "InternalExams",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Homeworks_AcademicTermId",
                table: "Homeworks",
                column: "AcademicTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Homeworks_ClassId",
                table: "Homeworks",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_ClassId",
                table: "CalendarEvents",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentParents_ParentId",
                table: "StudentParents",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentParents_StudentId",
                table: "StudentParents",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Classes_ClassId",
                table: "CalendarEvents",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Homeworks_AcademicTerms_AcademicTermId",
                table: "Homeworks",
                column: "AcademicTermId",
                principalTable: "AcademicTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Homeworks_Classes_ClassId",
                table: "Homeworks",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalExams_AcademicTerms_AcademicTermId",
                table: "InternalExams",
                column: "AcademicTermId",
                principalTable: "AcademicTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalExams_Classes_ClassId",
                table: "InternalExams",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentInstallments_PaymentInstallmentId",
                table: "Payments",
                column: "PaymentInstallmentId",
                principalTable: "PaymentInstallments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentPlans_PaymentPlanId",
                table: "Payments",
                column: "PaymentPlanId",
                principalTable: "PaymentPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClassAssignments_AcademicTerms_AcademicTermId",
                table: "StudentClassAssignments",
                column: "AcademicTermId",
                principalTable: "AcademicTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklySchedules_AcademicTerms_AcademicTermId",
                table: "WeeklySchedules",
                column: "AcademicTermId",
                principalTable: "AcademicTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Classes_ClassId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Homeworks_AcademicTerms_AcademicTermId",
                table: "Homeworks");

            migrationBuilder.DropForeignKey(
                name: "FK_Homeworks_Classes_ClassId",
                table: "Homeworks");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalExams_AcademicTerms_AcademicTermId",
                table: "InternalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalExams_Classes_ClassId",
                table: "InternalExams");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentInstallments_PaymentInstallmentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentPlans_PaymentPlanId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClassAssignments_AcademicTerms_AcademicTermId",
                table: "StudentClassAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklySchedules_AcademicTerms_AcademicTermId",
                table: "WeeklySchedules");

            migrationBuilder.DropTable(
                name: "StudentParents");

            migrationBuilder.DropIndex(
                name: "IX_WeeklySchedules_AcademicTermId",
                table: "WeeklySchedules");

            migrationBuilder.DropIndex(
                name: "IX_StudentClassAssignments_AcademicTermId",
                table: "StudentClassAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentInstallmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentPlanId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_InternalExams_AcademicTermId",
                table: "InternalExams");

            migrationBuilder.DropIndex(
                name: "IX_InternalExams_ClassId",
                table: "InternalExams");

            migrationBuilder.DropIndex(
                name: "IX_Homeworks_AcademicTermId",
                table: "Homeworks");

            migrationBuilder.DropIndex(
                name: "IX_Homeworks_ClassId",
                table: "Homeworks");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_ClassId",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "AcademicTermId",
                table: "WeeklySchedules");

            migrationBuilder.DropColumn(
                name: "AcademicTermId",
                table: "StudentClassAssignments");

            migrationBuilder.DropColumn(
                name: "PaymentInstallmentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentPlanId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "WorkPhone",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "AcademicTermId",
                table: "InternalExams");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "InternalExams");

            migrationBuilder.DropColumn(
                name: "AcademicTermId",
                table: "Homeworks");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Homeworks");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "CalendarEvents");

            migrationBuilder.AddColumn<bool>(
                name: "IsParentsSeparated",
                table: "Parents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ParentType",
                table: "Parents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "Parents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Parents_StudentId",
                table: "Parents",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Students_StudentId",
                table: "Parents",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
