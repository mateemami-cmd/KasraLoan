using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallmentPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaidMethod",
                table: "LoanInstallments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstallmentPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanInstallmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ChequeImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChequeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChequeBankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GatewayAuthority = table.Column<Guid>(type: "uuid", nullable: true),
                    GatewayRefId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GatewayExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallmentPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstallmentPayments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstallmentPayments_LoanInstallments_LoanInstallmentId",
                        column: x => x.LoanInstallmentId,
                        principalTable: "LoanInstallments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPayments_EmployeeId",
                table: "InstallmentPayments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPayments_GatewayAuthority",
                table: "InstallmentPayments",
                column: "GatewayAuthority",
                unique: true,
                filter: "\"GatewayAuthority\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPayments_LoanInstallmentId_CreatedAt",
                table: "InstallmentPayments",
                columns: new[] { "LoanInstallmentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPayments_Status",
                table: "InstallmentPayments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstallmentPayments");

            migrationBuilder.DropColumn(
                name: "PaidMethod",
                table: "LoanInstallments");
        }
    }
}
