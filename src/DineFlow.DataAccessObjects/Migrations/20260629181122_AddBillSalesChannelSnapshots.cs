using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddBillSalesChannelSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalesChannelCodeSnapshot",
                table: "Bills",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SalesChannelId",
                table: "Bills",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SalesChannelNameSnapshot",
                table: "Bills",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePriceSnapshot",
                table: "BillDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ChoiceExtraPriceSnapshot",
                table: "BillDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MenuItemChannelExtraPriceSnapshot",
                table: "BillDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SalesChannelId",
                table: "BillDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                INSERT INTO "SalesChannels" ("ChannelCode", "ChannelName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
                SELECT 'DINE_IN', 'Tại quán', TRUE, FALSE, NOW(), NOW()
                WHERE NOT EXISTS (
                    SELECT 1 FROM "SalesChannels"
                    WHERE "ChannelCode" = 'DINE_IN' AND "IsDeleted" = FALSE
                );
                """);

            migrationBuilder.Sql("""
                UPDATE "Bills"
                SET
                    "SalesChannelId" = channel."SalesChannelId",
                    "SalesChannelCodeSnapshot" = channel."ChannelCode",
                    "SalesChannelNameSnapshot" = channel."ChannelName"
                FROM (
                    SELECT "SalesChannelId", "ChannelCode", "ChannelName"
                    FROM "SalesChannels"
                    WHERE "ChannelCode" = 'DINE_IN' AND "IsDeleted" = FALSE
                    ORDER BY "SalesChannelId"
                    LIMIT 1
                ) AS channel
                WHERE "Bills"."SalesChannelId" = 0;
                """);

            migrationBuilder.Sql("""
                UPDATE "BillDetails"
                SET
                    "SalesChannelId" = bill."SalesChannelId",
                    "BasePriceSnapshot" = "BillDetails"."UnitPrice",
                    "MenuItemChannelExtraPriceSnapshot" = 0,
                    "ChoiceExtraPriceSnapshot" = 0
                FROM "Bills" AS bill
                WHERE "BillDetails"."BillId" = bill."BillId"
                  AND "BillDetails"."SalesChannelId" = 0;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_SalesChannelId",
                table: "Bills",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_SalesChannelId",
                table: "BillDetails",
                column: "SalesChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillDetails_SalesChannels_SalesChannelId",
                table: "BillDetails",
                column: "SalesChannelId",
                principalTable: "SalesChannels",
                principalColumn: "SalesChannelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_SalesChannels_SalesChannelId",
                table: "Bills",
                column: "SalesChannelId",
                principalTable: "SalesChannels",
                principalColumn: "SalesChannelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillDetails_SalesChannels_SalesChannelId",
                table: "BillDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_SalesChannels_SalesChannelId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_SalesChannelId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_BillDetails_SalesChannelId",
                table: "BillDetails");

            migrationBuilder.DropColumn(
                name: "SalesChannelCodeSnapshot",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "SalesChannelId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "SalesChannelNameSnapshot",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "BasePriceSnapshot",
                table: "BillDetails");

            migrationBuilder.DropColumn(
                name: "ChoiceExtraPriceSnapshot",
                table: "BillDetails");

            migrationBuilder.DropColumn(
                name: "MenuItemChannelExtraPriceSnapshot",
                table: "BillDetails");

            migrationBuilder.DropColumn(
                name: "SalesChannelId",
                table: "BillDetails");
        }
    }
}
