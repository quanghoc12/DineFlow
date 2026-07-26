using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class RandomizeTableQrTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Replace all predictable QR-TABLE-XXX tokens with cryptographically random UUIDs.
            // gen_random_uuid() is available in PostgreSQL 13+ via pgcrypto (or built-in).
            // replace(..., '-', '') mirrors Guid.NewGuid().ToString("N") format used in application code.
            migrationBuilder.Sql("""
                UPDATE "DiningTables"
                SET    "QrToken" = replace(gen_random_uuid()::text, '-', '')
                WHERE  "QrToken" ~ '^QR-TABLE-\d+$';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Tokens cannot be reverted to their original predictable values;
            // rolling back is intentionally a no-op.
        }
    }
}
