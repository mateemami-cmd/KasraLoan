using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPositionsAndSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MonthlyFeePercent",
                table: "LoanRequests",
                newName: "AnnualFeePercent");

            migrationBuilder.AddColumn<int>(
                name: "JobPositionId",
                table: "Employees",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MonthlySalary",
                table: "Employees",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseSalary = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_JobPositionId",
                table: "Employees",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPositions_Title",
                table: "JobPositions",
                column: "Title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_JobPositions_JobPositionId",
                table: "Employees",
                column: "JobPositionId",
                principalTable: "JobPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_JobPositions_JobPositionId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "JobPositions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_JobPositionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "JobPositionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MonthlySalary",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "AnnualFeePercent",
                table: "LoanRequests",
                newName: "MonthlyFeePercent");
        }
    }
}
