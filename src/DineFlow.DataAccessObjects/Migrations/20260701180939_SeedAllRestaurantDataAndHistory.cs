using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class SeedAllRestaurantDataAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = typeof(SeedAllRestaurantDataAndHistory).Assembly;
            var resourceName = "DineFlow.DataAccessObjects.Migrations.seed_data.sql";
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
            migrationBuilder.Sql("TRUNCATE TABLE \"Payments\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"BillDetailAdjustments\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"BillDetails\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"Bills\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"ServiceRequests\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"OrderItemSelectedChoices\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"OrderItems\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"Orders\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"TableSessionCustomers\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"TableSessions\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"ChoiceItemChannelPrices\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"MenuItemChannelPrices\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"SalesChannels\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"MenuItemChoiceGroups\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"ChoiceItems\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"ChoiceGroups\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"MenuItems\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"Categories\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"DiningTables\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"Users\" CASCADE;");
            migrationBuilder.Sql("TRUNCATE TABLE \"Areas\" CASCADE;");
        }
    }
}
