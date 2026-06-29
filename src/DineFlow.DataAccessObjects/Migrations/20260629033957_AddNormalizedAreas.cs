using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bills_TableSessionId",
                table: "Bills");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "DiningTables",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    AreaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AreaName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.AreaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_AreaId",
                table: "DiningTables",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_TableSessionId",
                table: "Bills",
                column: "TableSessionId",
                unique: true,
                filter: "\"IsDefault\" = TRUE AND \"Status\" = 'Unpaid'");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_AreaName",
                table: "Areas",
                column: "AreaName",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "Areas" ("AreaName", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt")
                SELECT DISTINCT BTRIM("Area"), 0, TRUE, NOW(), NOW()
                FROM "DiningTables"
                WHERE NULLIF(BTRIM("Area"), '') IS NOT NULL
                ON CONFLICT ("AreaName") DO NOTHING;

                UPDATE "DiningTables" AS d
                SET "AreaId" = a."AreaId"
                FROM "Areas" AS a
                WHERE LOWER(BTRIM(d."Area")) = LOWER(BTRIM(a."AreaName"));
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_DiningTables_Areas_AreaId",
                table: "DiningTables",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiningTables_Areas_AreaId",
                table: "DiningTables");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_DiningTables_AreaId",
                table: "DiningTables");

            migrationBuilder.DropIndex(
                name: "IX_Bills_TableSessionId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "DiningTables");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_TableSessionId",
                table: "Bills",
                column: "TableSessionId");
        }
    }
}
