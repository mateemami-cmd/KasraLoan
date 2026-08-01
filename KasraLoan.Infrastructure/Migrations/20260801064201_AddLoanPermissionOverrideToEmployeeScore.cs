using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanPermissionOverrideToEmployeeScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "EmployeeScores");

            migrationBuilder.AddColumn<bool>(
                name: "HasLoanPermissionOverride",
                table: "EmployeeScores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ManualOverrideScore",
                table: "EmployeeScores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OverriddenAt",
                table: "EmployeeScores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PermissionGrantedAt",
                table: "EmployeeScores",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasLoanPermissionOverride",
                table: "EmployeeScores");

            migrationBuilder.DropColumn(
                name: "ManualOverrideScore",
                table: "EmployeeScores");

            migrationBuilder.DropColumn(
                name: "OverriddenAt",
                table: "EmployeeScores");

            migrationBuilder.DropColumn(
                name: "PermissionGrantedAt",
                table: "EmployeeScores");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "EmployeeScores",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
