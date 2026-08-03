using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasraLoan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSecondaryPhoneWithList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryPhoneNumber",
                table: "Employees");

            migrationBuilder.AddColumn<List<string>>(
                name: "AdditionalPhoneNumbers",
                table: "Employees",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalPhoneNumbers",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryPhoneNumber",
                table: "Employees",
                type: "text",
                nullable: true);
        }
    }
}
