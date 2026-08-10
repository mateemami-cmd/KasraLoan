using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanEarlySettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SettlementAmount",
                table: "LoanRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettlementDemandedAt",
                table: "LoanRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettlementDueDate",
                table: "LoanRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementReason",
                table: "LoanRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettlementAmount",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "SettlementDemandedAt",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "SettlementDueDate",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "SettlementReason",
                table: "LoanRequests");
        }
    }
}
