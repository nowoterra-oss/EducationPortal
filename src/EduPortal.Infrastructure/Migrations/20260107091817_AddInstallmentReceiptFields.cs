using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallmentReceiptFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "PaymentInstallments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "PaymentInstallments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptPath",
                table: "PaymentInstallments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceiptUploadDate",
                table: "PaymentInstallments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "PaymentInstallments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInstallments_ApprovedBy",
                table: "PaymentInstallments",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInstallments_AspNetUsers_ApprovedBy",
                table: "PaymentInstallments",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInstallments_AspNetUsers_ApprovedBy",
                table: "PaymentInstallments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInstallments_ApprovedBy",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "ReceiptPath",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "ReceiptUploadDate",
                table: "PaymentInstallments");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "PaymentInstallments");
        }
    }
}
