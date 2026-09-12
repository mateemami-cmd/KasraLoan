using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuarantorToLoanRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GuaranteeChequeAcknowledged",
                table: "LoanRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "GuarantorEmployeeId",
                table: "LoanRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_GuarantorEmployeeId",
                table: "LoanRequests",
                column: "GuarantorEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_Employees_GuarantorEmployeeId",
                table: "LoanRequests",
                column: "GuarantorEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_Employees_GuarantorEmployeeId",
                table: "LoanRequests");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_GuarantorEmployeeId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "GuaranteeChequeAcknowledged",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "GuarantorEmployeeId",
                table: "LoanRequests");
        }
    }
}
