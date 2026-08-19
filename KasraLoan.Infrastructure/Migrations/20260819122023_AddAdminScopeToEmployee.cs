using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminScopeToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeniorAdmin",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ManagedLoanTypeId",
                table: "Employees",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagedLoanTypeId",
                table: "Employees",
                column: "ManagedLoanTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_LoanTypes_ManagedLoanTypeId",
                table: "Employees",
                column: "ManagedLoanTypeId",
                principalTable: "LoanTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_LoanTypes_ManagedLoanTypeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ManagedLoanTypeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsSeniorAdmin",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ManagedLoanTypeId",
                table: "Employees");
        }
    }
}
