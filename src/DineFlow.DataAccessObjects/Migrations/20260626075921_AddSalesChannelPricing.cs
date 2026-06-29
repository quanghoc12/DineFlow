using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesChannelPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalOrderCode",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesChannelId",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ChannelExtraPriceSnapshot",
                table: "OrderItemSelectedChoices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalExtraPriceSnapshot",
                table: "OrderItemSelectedChoices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ChannelExtraPriceSnapshot",
                table: "OrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalUnitPriceSnapshot",
                table: "OrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SalesChannels",
                columns: table => new
                {
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChannelName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesChannels", x => x.SalesChannelId);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceItemChannelPrices",
                columns: table => new
                {
                    ChoiceItemId = table.Column<int>(type: "integer", nullable: false),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceItemChannelPrices", x => new { x.ChoiceItemId, x.SalesChannelId });
                    table.ForeignKey(
                        name: "FK_ChoiceItemChannelPrices_ChoiceItems_ChoiceItemId",
                        column: x => x.ChoiceItemId,
                        principalTable: "ChoiceItems",
                        principalColumn: "ChoiceItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChoiceItemChannelPrices_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "SalesChannels" ("SalesChannelId", "ChannelCode", "ChannelName", "IsActive", "CreatedAt", "UpdatedAt")
                VALUES
                    (1, 'DINE_IN', 'Tai quan', TRUE, NOW(), NOW()),
                    (2, 'CUSTOMER_WEB', 'Khach quet QR', TRUE, NOW(), NOW()),
                    (3, 'SHOPEEFOOD', 'ShopeeFood', TRUE, NOW(), NOW()),
                    (4, 'GRABFOOD', 'GrabFood', TRUE, NOW(), NOW())
                ON CONFLICT ("SalesChannelId") DO NOTHING;

                SELECT setval(
                    pg_get_serial_sequence('"SalesChannels"', 'SalesChannelId'),
                    GREATEST((SELECT COALESCE(MAX("SalesChannelId"), 1) FROM "SalesChannels"), 1),
                    TRUE);

                UPDATE "Orders"
                SET "SalesChannelId" = 1
                WHERE "SalesChannelId" = 0;

                UPDATE "OrderItems"
                SET "FinalUnitPriceSnapshot" = "BasePriceSnapshot"
                WHERE "FinalUnitPriceSnapshot" = 0;

                UPDATE "OrderItemSelectedChoices"
                SET "FinalExtraPriceSnapshot" = "ExtraPriceSnapshot"
                WHERE "FinalExtraPriceSnapshot" = 0;
                """);

            migrationBuilder.CreateTable(
                name: "MenuItemChannelPrices",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemChannelPrices", x => new { x.MenuItemId, x.SalesChannelId });
                    table.ForeignKey(
                        name: "FK_MenuItemChannelPrices_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemChannelPrices_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SalesChannelId",
                table: "Orders",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceItemChannelPrices_SalesChannelId",
                table: "ChoiceItemChannelPrices",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemChannelPrices_SalesChannelId",
                table: "MenuItemChannelPrices",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesChannels_ChannelCode",
                table: "SalesChannels",
                column: "ChannelCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_SalesChannels_SalesChannelId",
                table: "Orders",
                column: "SalesChannelId",
                principalTable: "SalesChannels",
                principalColumn: "SalesChannelId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_SalesChannels_SalesChannelId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "ChoiceItemChannelPrices");

            migrationBuilder.DropTable(
                name: "MenuItemChannelPrices");

            migrationBuilder.DropTable(
                name: "SalesChannels");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SalesChannelId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExternalOrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SalesChannelId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ChannelExtraPriceSnapshot",
                table: "OrderItemSelectedChoices");

            migrationBuilder.DropColumn(
                name: "FinalExtraPriceSnapshot",
                table: "OrderItemSelectedChoices");

            migrationBuilder.DropColumn(
                name: "ChannelExtraPriceSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "FinalUnitPriceSnapshot",
                table: "OrderItems");
        }
    }
}
