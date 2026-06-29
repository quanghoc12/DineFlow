using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DineFlow.DataAccessObjects.Migrations
{
    /// <inheritdoc />
    public partial class CurrentDineFlowSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceGroups",
                columns: table => new
                {
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MaxSelectDefault = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceGroups", x => x.ChoiceGroupId);
                });

            migrationBuilder.CreateTable(
                name: "SalesChannels",
                columns: table => new
                {
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChannelName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesChannels", x => x.SalesChannelId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DiningTables",
                columns: table => new
                {
                    TableId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AreaId = table.Column<int>(type: "integer", nullable: true),
                    Area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QrToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiningTables", x => x.TableId);
                    table.ForeignKey(
                        name: "FK_DiningTables_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "AreaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOutOfStock = table.Column<bool>(type: "boolean", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.MenuItemId);
                    table.ForeignKey(
                        name: "FK_MenuItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChoiceItems",
                columns: table => new
                {
                    ChoiceItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceItems", x => x.ChoiceItemId);
                    table.ForeignKey(
                        name: "FK_ChoiceItems_ChoiceGroups_ChoiceGroupId",
                        column: x => x.ChoiceGroupId,
                        principalTable: "ChoiceGroups",
                        principalColumn: "ChoiceGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSessions",
                columns: table => new
                {
                    TableSessionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OpenedBy = table.Column<int>(type: "integer", nullable: true),
                    ClosedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSessions", x => x.TableSessionId);
                    table.ForeignKey(
                        name: "FK_TableSessions_DiningTables_TableId",
                        column: x => x.TableId,
                        principalTable: "DiningTables",
                        principalColumn: "TableId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "MenuItemChoiceGroups",
                columns: table => new
                {
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    MaxSelect = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemChoiceGroups", x => new { x.MenuItemId, x.ChoiceGroupId });
                    table.ForeignKey(
                        name: "FK_MenuItemChoiceGroups_ChoiceGroups_ChoiceGroupId",
                        column: x => x.ChoiceGroupId,
                        principalTable: "ChoiceGroups",
                        principalColumn: "ChoiceGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemChoiceGroups_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    BillId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    SalesChannelCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SalesChannelNameSnapshot = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BillNo = table.Column<int>(type: "integer", nullable: false),
                    BillName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<int>(type: "integer", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.BillId);
                    table.ForeignKey(
                        name: "FK_Bills_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bills_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSessionCustomers",
                columns: table => new
                {
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    ClientToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSessionCustomers", x => x.SessionCustomerId);
                    table.ForeignKey(
                        name: "FK_TableSessionCustomers_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillDetailAdjustments",
                columns: table => new
                {
                    BillDetailAdjustmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    BillDetailId = table.Column<int>(type: "integer", nullable: false),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    QuantityBefore = table.Column<int>(type: "integer", nullable: false),
                    QuantityAfter = table.Column<int>(type: "integer", nullable: false),
                    ChangedQuantity = table.Column<int>(type: "integer", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDetailAdjustments", x => x.BillDetailAdjustmentId);
                    table.ForeignKey(
                        name: "FK_BillDetailAdjustments_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillDetails",
                columns: table => new
                {
                    BillDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ChoiceSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    NotifiedQuantity = table.Column<int>(type: "integer", nullable: false),
                    BasePriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MenuItemChannelExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChoiceExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillDetails", x => x.BillDetailId);
                    table.ForeignKey(
                        name: "FK_BillDetails_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillDetails_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillDetails_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedBy = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: true),
                    SalesChannelId = table.Column<int>(type: "integer", nullable: false),
                    OrderCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrderSource = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExternalOrderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClientToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PrintStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CustomerNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SystemNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrintError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CancelledBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_SalesChannels_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalTable: "SalesChannels",
                        principalColumn: "SalesChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_TableSessionCustomers_SessionCustomerId",
                        column: x => x.SessionCustomerId,
                        principalTable: "TableSessionCustomers",
                        principalColumn: "SessionCustomerId");
                    table.ForeignKey(
                        name: "FK_Orders_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableSessionId = table.Column<int>(type: "integer", nullable: false),
                    SessionCustomerId = table.Column<int>(type: "integer", nullable: true),
                    ClientToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_TableSessionCustomers_SessionCustomerId",
                        column: x => x.SessionCustomerId,
                        principalTable: "TableSessionCustomers",
                        principalColumn: "SessionCustomerId");
                    table.ForeignKey(
                        name: "FK_ServiceRequests_TableSessions_TableSessionId",
                        column: x => x.TableSessionId,
                        principalTable: "TableSessions",
                        principalColumn: "TableSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    MenuItemId = table.Column<int>(type: "integer", nullable: false),
                    MenuItemNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BasePriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChannelExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalUnitPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemSelectedChoices",
                columns: table => new
                {
                    OrderItemSelectedChoiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    ChoiceItemId = table.Column<int>(type: "integer", nullable: false),
                    GroupNameSnapshot = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ChoiceNameSnapshot = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChannelExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalExtraPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemSelectedChoices", x => x.OrderItemSelectedChoiceId);
                    table.ForeignKey(
                        name: "FK_OrderItemSelectedChoices_ChoiceGroups_ChoiceGroupId",
                        column: x => x.ChoiceGroupId,
                        principalTable: "ChoiceGroups",
                        principalColumn: "ChoiceGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemSelectedChoices_ChoiceItems_ChoiceItemId",
                        column: x => x.ChoiceItemId,
                        principalTable: "ChoiceItems",
                        principalColumn: "ChoiceItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemSelectedChoices_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Areas_AreaName",
                table: "Areas",
                column: "AreaName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillDetailAdjustments_BillDetailId",
                table: "BillDetailAdjustments",
                column: "BillDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetailAdjustments_BillId",
                table: "BillDetailAdjustments",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetailAdjustments_MenuItemId",
                table: "BillDetailAdjustments",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_BillId",
                table: "BillDetails",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_MenuItemId",
                table: "BillDetails",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_SalesChannelId",
                table: "BillDetails",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_BillCode",
                table: "Bills",
                column: "BillCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_SalesChannelId",
                table: "Bills",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_TableSessionId",
                table: "Bills",
                column: "TableSessionId",
                unique: true,
                filter: "\"IsDefault\" = TRUE AND \"Status\" = 'Unpaid'");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceItemChannelPrices_SalesChannelId",
                table: "ChoiceItemChannelPrices",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceItems_ChoiceGroupId",
                table: "ChoiceItems",
                column: "ChoiceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_AreaId",
                table: "DiningTables",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_QrToken",
                table: "DiningTables",
                column: "QrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemChannelPrices_SalesChannelId",
                table: "MenuItemChannelPrices",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemChoiceGroups_ChoiceGroupId",
                table: "MenuItemChoiceGroups",
                column: "ChoiceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemSelectedChoices_ChoiceGroupId",
                table: "OrderItemSelectedChoices",
                column: "ChoiceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemSelectedChoices_ChoiceItemId",
                table: "OrderItemSelectedChoices",
                column: "ChoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemSelectedChoices_OrderItemId",
                table: "OrderItemSelectedChoices",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SalesChannelId",
                table: "Orders",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SessionCustomerId",
                table: "Orders",
                column: "SessionCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableSessionId",
                table: "Orders",
                column: "TableSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BillId",
                table: "Payments",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesChannels_ChannelCode",
                table: "SalesChannels",
                column: "ChannelCode",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_SessionCustomerId",
                table: "ServiceRequests",
                column: "SessionCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_TableSessionId",
                table: "ServiceRequests",
                column: "TableSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TableSessionCustomers_TableSessionId_ClientToken",
                table: "TableSessionCustomers",
                columns: new[] { "TableSessionId", "ClientToken" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_TableId",
                table: "TableSessions",
                column: "TableId",
                unique: true,
                filter: "\"Status\" IN ('Browsing', 'Open', 'WaitingPayment')");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillDetailAdjustments");

            migrationBuilder.DropTable(
                name: "BillDetails");

            migrationBuilder.DropTable(
                name: "ChoiceItemChannelPrices");

            migrationBuilder.DropTable(
                name: "MenuItemChannelPrices");

            migrationBuilder.DropTable(
                name: "MenuItemChoiceGroups");

            migrationBuilder.DropTable(
                name: "OrderItemSelectedChoices");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ChoiceItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "ChoiceGroups");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "SalesChannels");

            migrationBuilder.DropTable(
                name: "TableSessionCustomers");

            migrationBuilder.DropTable(
                name: "TableSessions");

            migrationBuilder.DropTable(
                name: "DiningTables");

            migrationBuilder.DropTable(
                name: "Areas");
        }
    }
}
