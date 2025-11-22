using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPlanSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInstallments_PaymentPlans_PaymentPlanId",
                table: "PaymentInstallments");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentPlans_Students_StudentId",
                table: "PaymentPlans");

            migrationBuilder.DropIndex(
                name: "IX_PaymentPlans_StudentId",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PaymentPlans");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "PaymentPlans",
                newName: "DaysBetweenInstallments");

            migrationBuilder.RenameColumn(
                name: "ReceiptUrl",
                table: "PaymentInstallments",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "PaymentPlanId",
                table: "PaymentInstallments",
                newName: "StudentPaymentPlanId");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "PaymentInstallments",
                newName: "PaymentId");

            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "PaymentInstallments",
                newName: "PaidDate");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInstallments_PaymentPlanId",
                table: "PaymentInstallments",
                newName: "IX_PaymentInstallments_StudentPaymentPlanId");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCost",
                table: "StudyAbroadPrograms",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PaymentPlans",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DownPaymentDiscount",
                table: "PaymentPlans",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PaymentPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PaymentPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "PaymentPlans",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PaymentInstallments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "PaymentInstallments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PaymentInstallments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentPaymentPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    PaymentPlanId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PackagePurchaseId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPaymentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPaymentPlans_PaymentPlans_PaymentPlanId",
                        column: x => x.PaymentPlanId,
                        principalTable: "PaymentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentPaymentPlans_StudentPackagePurchases_PackagePurchaseId",
                        column: x => x.PackagePurchaseId,
                        principalTable: "StudentPackagePurchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentPaymentPlans_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInstallments_PaymentId",
                table: "PaymentInstallments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPaymentPlans_PackagePurchaseId",
                table: "StudentPaymentPlans",
                column: "PackagePurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPaymentPlans_PaymentPlanId",
                table: "StudentPaymentPlans",
                column: "PaymentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPaymentPlans_StudentId",
                table: "StudentPaymentPlans",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInstallments_Payments_PaymentId",
                table: "PaymentInstallments",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInstallments_StudentPaymentPlans_StudentPaymentPlanId",
                table: "PaymentInstallments",
                column: "StudentPaymentPlanId",
                principalTable: "StudentPaymentPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInstallments_Payments_PaymentId",
                table: "PaymentInstallments");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInstallments_StudentPaymentPlans_StudentPaymentPlanId",
                table: "PaymentInstallments");

            migrationBuilder.DropTable(
                name: "StudentPaymentPlans");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInstallments_PaymentId",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "ActualCost",
                table: "StudyAbroadPrograms");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "DownPaymentDiscount",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "PaymentPlans");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PaymentInstallments");

            migrationBuilder.RenameColumn(
                name: "DaysBetweenInstallments",
                table: "PaymentPlans",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "StudentPaymentPlanId",
                table: "PaymentInstallments",
                newName: "PaymentPlanId");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "PaymentInstallments",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "PaidDate",
                table: "PaymentInstallments",
                newName: "PaymentDate");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "PaymentInstallments",
                newName: "ReceiptUrl");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInstallments_StudentPaymentPlanId",
                table: "PaymentInstallments",
                newName: "IX_PaymentInstallments_PaymentPlanId");

            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "PaymentPlans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PaymentPlans",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PaymentPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PaymentPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PaymentPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPlans_StudentId",
                table: "PaymentPlans",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInstallments_PaymentPlans_PaymentPlanId",
                table: "PaymentInstallments",
                column: "PaymentPlanId",
                principalTable: "PaymentPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentPlans_Students_StudentId",
                table: "PaymentPlans",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
