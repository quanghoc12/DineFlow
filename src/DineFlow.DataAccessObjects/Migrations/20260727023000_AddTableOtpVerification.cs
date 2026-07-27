using System;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260727023000_AddTableOtpVerification")]
    public partial class AddTableOtpVerification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentOtp",
                table: "DiningTables",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpUpdatedAt",
                table: "DiningTables",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "TableSessionCustomers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "TableSessionCustomers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "DiningTables"
                SET "CurrentOtp" = (
                    SELECT string_agg(substr(chars, floor(random() * length(chars) + 1)::int, 1), '')
                    FROM generate_series(1, 6 + "DiningTables"."TableId" * 0), (
                        SELECT 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789' AS chars
                    ) source
                ),
                "OtpUpdatedAt" = NOW()
                WHERE "CurrentOtp" = '';
                """);

            migrationBuilder.Sql("""
                UPDATE "TableSessionCustomers" customer
                SET "IsVerified" = TRUE,
                    "VerifiedAt" = COALESCE(customer."VerifiedAt", NOW())
                FROM "TableSessions" session
                WHERE customer."TableSessionId" = session."TableSessionId"
                  AND session."Status" IN ('Open', 'WaitingPayment');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentOtp",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "OtpUpdatedAt",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "TableSessionCustomers");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "TableSessionCustomers");
        }
    }
}
