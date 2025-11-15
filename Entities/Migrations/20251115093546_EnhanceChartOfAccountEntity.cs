using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuFlow.Entities.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceChartOfAccountEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChartOfAccounts",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "AccountCode",
                table: "ChartOfAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ChartOfAccounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "IDR");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChartOfAccounts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsHeader",
                table: "ChartOfAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "ChartOfAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NormalBalance",
                table: "ChartOfAccounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Debit");

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalance",
                table: "ChartOfAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_AccountType",
                table: "ChartOfAccounts",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_IsActive",
                table: "ChartOfAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_IsDeleted",
                table: "ChartOfAccounts",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChartOfAccounts_AccountType",
                table: "ChartOfAccounts");

            migrationBuilder.DropIndex(
                name: "IX_ChartOfAccounts_IsActive",
                table: "ChartOfAccounts");

            migrationBuilder.DropIndex(
                name: "IX_ChartOfAccounts_IsDeleted",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "IsHeader",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "NormalBalance",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "OpeningBalance",
                table: "ChartOfAccounts");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ChartOfAccounts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountCode",
                table: "ChartOfAccounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
