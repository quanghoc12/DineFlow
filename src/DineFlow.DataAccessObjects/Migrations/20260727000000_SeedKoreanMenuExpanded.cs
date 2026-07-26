using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class SeedKoreanMenuExpanded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = typeof(SeedKoreanMenuExpanded).Assembly;
            var resourceName = "DineFlow.DataAccessObjects.Migrations.SeedKoreanMenuExpanded.sql";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new System.Exception($"Embedded resource '{resourceName}' not found.");
                }
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var sql = reader.ReadToEnd();
                    migrationBuilder.Sql(sql);
                }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Idempotent migration - Down is intentionally no-op to protect production data
        }
    }
}
