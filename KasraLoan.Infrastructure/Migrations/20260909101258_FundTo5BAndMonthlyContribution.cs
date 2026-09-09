using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FundTo5BAndMonthlyContribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastContributionPeriod",
                table: "LoanFunds",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "LoanFunds",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Balance", "LastContributionPeriod" },
                values: new object[] { 5000000000L, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastContributionPeriod",
                table: "LoanFunds");

            migrationBuilder.UpdateData(
                table: "LoanFunds",
                keyColumn: "Id",
                keyValue: 1,
                column: "Balance",
                value: 1000000000L);
        }
    }
}
