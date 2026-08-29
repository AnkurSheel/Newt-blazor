using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetWorthTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ClosedDate = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.CheckConstraint("CK_Account_Dates", "[ClosedDate] IS NULL OR [ClosedDate] > [OpenDate]");
                    table.CheckConstraint("CK_Account_Name_Length", "length([Name]) <= 256");
                    table.CheckConstraint("CK_Account_Type_Values", "[Type] IN (1, 2)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_Name",
                table: "accounts",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
