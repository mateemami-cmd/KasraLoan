using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelLoanDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "LoanRequests",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "LoanRequests");
        }
    }
}
