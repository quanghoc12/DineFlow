using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260629141324_AddMenuItemIsOutOfStock")]
    public partial class AddMenuItemIsOutOfStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty.
            // IsOutOfStock is created in AddMenuItemSoftDelete so the following
            // AddSalesChannelSoftDelete migration can safely alter it on a clean database.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty; the column is owned by AddMenuItemSoftDelete.
        }
    }
}
