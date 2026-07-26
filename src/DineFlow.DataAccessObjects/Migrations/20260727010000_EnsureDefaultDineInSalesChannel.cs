using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class EnsureDefaultDineInSalesChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    v_now TIMESTAMP WITH TIME ZONE := NOW();
                    v_dine_in_id INT;
                BEGIN
                    SELECT "SalesChannelId"
                    INTO v_dine_in_id
                    FROM "SalesChannels"
                    WHERE "ChannelCode" = 'DINE_IN'
                    ORDER BY "SalesChannelId"
                    LIMIT 1;

                    IF v_dine_in_id IS NULL THEN
                        INSERT INTO "SalesChannels" ("ChannelCode", "ChannelName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
                        VALUES ('DINE_IN', 'Kênh bán tại quán', true, false, v_now, v_now)
                        RETURNING "SalesChannelId" INTO v_dine_in_id;
                    ELSE
                        UPDATE "SalesChannels"
                        SET "IsActive" = false,
                            "IsDeleted" = true,
                            "UpdatedAt" = v_now
                        WHERE "ChannelCode" = 'DINE_IN'
                          AND "SalesChannelId" <> v_dine_in_id;

                        UPDATE "SalesChannels"
                        SET "ChannelName" = 'Kênh bán tại quán',
                            "IsActive" = true,
                            "IsDeleted" = false,
                            "UpdatedAt" = v_now
                        WHERE "SalesChannelId" = v_dine_in_id;
                    END IF;

                    INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt")
                    SELECT item."MenuItemId", v_dine_in_id, 0, v_now, v_now
                    FROM "MenuItems" item
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM "MenuItemChannelPrices" price
                        WHERE price."MenuItemId" = item."MenuItemId"
                          AND price."SalesChannelId" = v_dine_in_id
                    );

                    INSERT INTO "ChoiceItemChannelPrices" ("ChoiceItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt")
                    SELECT choice_item."ChoiceItemId", v_dine_in_id, 0, v_now, v_now
                    FROM "ChoiceItems" choice_item
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM "ChoiceItemChannelPrices" price
                        WHERE price."ChoiceItemId" = choice_item."ChoiceItemId"
                          AND price."SalesChannelId" = v_dine_in_id
                    );
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Production data repair migration. Down is intentionally no-op.
        }
    }
}
