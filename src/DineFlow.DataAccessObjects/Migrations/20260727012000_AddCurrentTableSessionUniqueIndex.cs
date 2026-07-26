using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentTableSessionUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_TableSessions_OneCurrentPerTable"
                ON "TableSessions" ("TableId")
                WHERE "Status" IN ('Browsing', 'Open', 'WaitingPayment');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_TableSessions_OneCurrentPerTable";
                """);
        }
    }
}
