using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesChannelSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesChannels_ChannelCode",
                table: "SalesChannels");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SalesChannels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsOutOfStock",
                table: "MenuItems",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SalesChannels_ChannelCode",
                table: "SalesChannels",
                column: "ChannelCode",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesChannels_ChannelCode",
                table: "SalesChannels");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SalesChannels");

            migrationBuilder.AlterColumn<bool>(
                name: "IsOutOfStock",
                table: "MenuItems",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "IX_SalesChannels_ChannelCode",
                table: "SalesChannels",
                column: "ChannelCode",
                unique: true);
        }
    }
}
