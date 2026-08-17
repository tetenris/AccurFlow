using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccuFlow.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddYearEndClosingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YearEndClosings",
                columns: table => new
                {
                    ClosingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingJournalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearEndClosings", x => x.ClosingId);
                    table.ForeignKey(
                        name: "FK_YearEndClosings_JournalEntries_ClosingJournalId",
                        column: x => x.ClosingJournalId,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YearEndClosings_ClosingJournalId",
                table: "YearEndClosings",
                column: "ClosingJournalId");

            migrationBuilder.CreateIndex(
                name: "IX_YearEndClosings_FiscalYear",
                table: "YearEndClosings",
                column: "FiscalYear",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YearEndClosings");
        }
    }
}
