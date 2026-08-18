using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralLedgerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_AccountId_JournalId",
                table: "JournalLines",
                columns: new[] { "AccountId", "JournalId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalLines_AccountId_JournalId",
                table: "JournalLines");
        }
    }
}

