using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Tables;
using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Bills;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace DineFlow.DataAccessObjects.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedDevelopmentDataAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        // Seeding is now handled via EF Core Migrations (SeedAllRestaurantDataAndHistory)
        return;

        // Only run clean-slate seeding if the Korean menu is not yet present
        bool alreadySeeded = await dbContext.Categories.AnyAsync(x => x.CategoryName == "K-BBQ (Thịt nướng)", cancellationToken);
        if (alreadySeeded)
        {
            bool billsEmpty = !await dbContext.Bills.AnyAsync(cancellationToken);
            if (billsEmpty)
            {
                Console.WriteLine("Korean restaurant data is seeded but Bills are empty. Seeding historical data...");
                await SeedHistoricalDataAsync(dbContext, cancellationToken);
            }
            else
            {
                Console.WriteLine("Korean restaurant data and historical bills are already seeded. Skipping seeder for instant startup.");
            }
            return;
        }

        // Clean up all old data to prevent foreign key conflicts and clean-slate seed
        await ClearAllDataAsync(dbContext, cancellationToken);

        DateTime now = DateTime.UtcNow;

        Dictionary<string, User> users = await SeedUsersAsync(dbContext, now, cancellationToken);
        Dictionary<string, DiningTable> tables = await SeedTablesAsync(dbContext, now, cancellationToken);
        Dictionary<string, Category> categories = await SeedCategoriesAsync(dbContext, now, cancellationToken);
        Dictionary<string, SalesChannel> salesChannels = await SeedSalesChannelsAsync(dbContext, now, cancellationToken);
        Dictionary<string, MenuItem> menuItems = await SeedMenuItemsAsync(dbContext, categories, now, cancellationToken);
        Dictionary<string, ChoiceGroup> choiceGroups = await SeedChoiceGroupsAsync(dbContext, now, cancellationToken);
        Dictionary<string, ChoiceItem> choiceItems = await SeedChoiceItemsAsync(dbContext, choiceGroups, now, cancellationToken);
        await SeedMenuItemChoiceGroupsAsync(dbContext, menuItems, choiceGroups, now, cancellationToken);
        await SeedMenuItemChannelPricesAsync(dbContext, menuItems, salesChannels, now, cancellationToken);
        await SeedChoiceItemChannelPricesAsync(dbContext, choiceItems, salesChannels, now, cancellationToken);

        Console.WriteLine("Seeding historical data...");
        await SeedHistoricalDataAsync(dbContext, cancellationToken);

        _ = users;
        _ = tables;
    }

    private static async Task ClearAllDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        Console.WriteLine("Clearing database mock tables...");

        string sql = """
            TRUNCATE TABLE
                "Payments",
                "BillDetailAdjustments",
                "BillDetails",
                "Bills",
                "ServiceRequests",
                "OrderItemSelectedChoices",
                "OrderItems",
                "Orders",
                "TableSessionCustomers",
                "TableSessions",
                "MenuItemChannelPrices",
                "ChoiceItemChannelPrices",
                "MenuItemChoiceGroups",
                "ChoiceItems",
                "ChoiceGroups",
                "MenuItems",
                "Categories",
                "DiningTables",
                "Areas",
                "SalesChannels",
                "Users"
            RESTART IDENTITY CASCADE;
            """;

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        Console.WriteLine("Cleanup completed successfully.");
    }

    private static async Task ClearTransactionDataOnlyAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        Console.WriteLine("Clearing database transaction tables only...");

        string sql = """
            TRUNCATE TABLE
                "Payments",
                "BillDetailAdjustments",
                "BillDetails",
                "Bills",
                "ServiceRequests",
                "OrderItemSelectedChoices",
                "OrderItems",
                "Orders",
                "TableSessionCustomers",
                "TableSessions"
            RESTART IDENTITY CASCADE;
            """;

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        Console.WriteLine("Transaction tables cleanup completed successfully.");
    }

    private static async Task<bool> HasCurrentSchemaAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        bool shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = """
                SELECT COUNT(*)
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = 'MenuItems'
                  AND column_name IN ('MenuItemId', 'CategoryId', 'Name', 'BasePrice', 'IsAvailable', 'Stock')
                """;

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            int menuColumnCount = Convert.ToInt32(result);

            command.CommandText = """
                SELECT COUNT(*)
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name IN ('SalesChannels', 'MenuItemChannelPrices', 'ChoiceItemChannelPrices')
                """;

            result = await command.ExecuteScalarAsync(cancellationToken);
            int channelTableCount = Convert.ToInt32(result);

            return menuColumnCount == 6 && channelTableCount == 3;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<Dictionary<string, SalesChannel>> SeedSalesChannelsAsync(
        AppDbContext dbContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string Code, string Name)[] seeds =
        [
            ("DINE_IN", "Kênh bán tại quán"),
            ("CUSTOMER_WEB", "Khách quét QR"),
            ("SHOPEEFOOD", "ShopeeFood"),
            ("GRABFOOD", "GrabFood")
        ];

        Dictionary<string, SalesChannel> channels = [];

        foreach ((string code, string name) in seeds)
        {
            SalesChannel? channel = await dbContext.SalesChannels.FirstOrDefaultAsync(
                x => x.ChannelCode == code,
                cancellationToken);

            if (channel is null)
            {
                channel = new SalesChannel
                {
                    ChannelCode = code,
                    ChannelName = name,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await dbContext.SalesChannels.AddAsync(channel, cancellationToken);
            }
            else
            {
                channel.ChannelName = name;
                channel.IsActive = true;
                channel.UpdatedAt = now;
            }

            channels[code] = channel;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return channels;
    }

    private static async Task<Dictionary<string, User>> SeedUsersAsync(
        AppDbContext dbContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        User owner = await GetOrCreateUserAsync(
            dbContext,
            username: "owner",
            passwordHash: "owner123",
            fullName: "Chủ nhà hàng",
            role: "Owner",
            now,
            cancellationToken);

        User admin = await GetOrCreateUserAsync(
            dbContext,
            username: "admin",
            passwordHash: "admin123",
            fullName: "Quản trị viên",
            role: "Admin",
            now,
            cancellationToken);

        User staff = await GetOrCreateUserAsync(
            dbContext,
            username: "staff01",
            passwordHash: "staff123",
            fullName: "Nhân viên 01",
            role: "Staff",
            now,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new Dictionary<string, User>
        {
            ["owner"] = owner,
            ["admin"] = admin,
            ["staff01"] = staff
        };
    }

    private static async Task<User> GetOrCreateUserAsync(
        AppDbContext dbContext,
        string username,
        string passwordHash,
        string fullName,
        string role,
        DateTime now,
        CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            FullName = fullName,
            Role = role,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await dbContext.Users.AddAsync(user, cancellationToken);
        return user;
    }

    private static async Task<Dictionary<string, DiningTable>> SeedTablesAsync(
        AppDbContext dbContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string AreaName, int DisplayOrder)[] areaSeeds =
        [
            ("Tầng 1 (Khu bàn lẻ)", 1),
            ("Tầng 2 (Gia đình)", 2),
            ("Tầng VIP (Phòng Hanok)", 3)
        ];

        Dictionary<string, Area> areas = [];
        foreach (var a in areaSeeds)
        {
            Area? area = await dbContext.Areas.FirstOrDefaultAsync(x => x.AreaName == a.AreaName, cancellationToken);
            if (area is null)
            {
                area = new Area
                {
                    AreaName = a.AreaName,
                    DisplayOrder = a.DisplayOrder,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await dbContext.Areas.AddAsync(area, cancellationToken);
            }
            areas[a.AreaName] = area;
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        (string Name, string Area, string Token)[] seeds =
        [
            ("Bàn 01", "Tầng 1 (Khu bàn lẻ)", "QR-TABLE-001"),
            ("Bàn 02", "Tầng 1 (Khu bàn lẻ)", "QR-TABLE-002"),
            ("Bàn 03", "Tầng 1 (Khu bàn lẻ)", "QR-TABLE-003"),
            ("Bàn 04", "Tầng 1 (Khu bàn lẻ)", "QR-TABLE-004"),
            ("Bàn 05", "Tầng 1 (Khu bàn lẻ)", "QR-TABLE-005"),
            ("Bàn 06", "Tầng 2 (Gia đình)", "QR-TABLE-006"),
            ("Bàn 07", "Tầng 2 (Gia đình)", "QR-TABLE-007"),
            ("Bàn 08", "Tầng 2 (Gia đình)", "QR-TABLE-008"),
            ("Bàn 09", "Tầng 2 (Gia đình)", "QR-TABLE-009"),
            ("Bàn 10", "Tầng 2 (Gia đình)", "QR-TABLE-010"),
            ("Bàn VIP 01", "Tầng VIP (Phòng Hanok)", "QR-VIP-001"),
            ("Bàn VIP 02", "Tầng VIP (Phòng Hanok)", "QR-VIP-002"),
            ("Bàn VIP 03", "Tầng VIP (Phòng Hanok)", "QR-VIP-003")
        ];

        Dictionary<string, DiningTable> tables = [];

        foreach ((string name, string areaName, string token) in seeds)
        {
            DiningTable? table = await dbContext.DiningTables.FirstOrDefaultAsync(x => x.QrToken == token, cancellationToken);

            if (table is null)
            {
                table = new DiningTable
                {
                    TableName = name,
                    Area = areaName,
                    AreaId = areas[areaName].AreaId,
                    QrToken = token,
                    Status = "Available",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await dbContext.DiningTables.AddAsync(table, cancellationToken);
            }
            else
            {
                table.TableName = name;
                table.Area = areaName;
                table.AreaId = areas[areaName].AreaId;
                table.UpdatedAt = now;
            }

            tables[token] = table;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return tables;
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(
        AppDbContext dbContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string Name, string Description, int DisplayOrder)[] seeds =
        [
            ("K-BBQ (Thịt nướng)", "Thịt bò và thịt heo nướng hảo hạng", 1),
            ("Lẩu & Canh (Soup & Hotpot)", "Canh truyền thống và lẩu Hàn Quốc ấm nóng", 2),
            ("Cơm & Mì (Main Rice & Noodles)", "Cơm trộn và mì đặc trưng xứ Hàn", 3),
            ("Ăn kèm & Panchan (Side dishes)", "Panchan và món khai vị ăn kèm hấp dẫn", 4),
            ("Đồ uống & Soju (Drinks)", "Rượu Soju, rượu gạo truyền thống và nước giải khát", 5)
        ];

        Dictionary<string, Category> categories = [];

        foreach ((string name, string description, int displayOrder) in seeds)
        {
            Category? category = await dbContext.Categories.FirstOrDefaultAsync(x => x.CategoryName == name, cancellationToken);

            if (category is null)
            {
                category = new Category
                {
                    CategoryName = name,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await dbContext.Categories.AddAsync(category, cancellationToken);
            }

            categories[name] = category;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return categories;
    }

    private static async Task<Dictionary<string, MenuItem>> SeedMenuItemsAsync(
        AppDbContext dbContext,
        IReadOnlyDictionary<string, Category> categories,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string Category, string Name, string Description, decimal Price, int Stock)[] seeds =
        [
            ("K-BBQ (Thịt nướng)", "Ba chỉ heo nướng Samgyeopsal", "Thịt ba chỉ heo tươi thái lát dày vừa, nướng thơm giòn ăn kèm kim chi.", 189000m, 50),
            ("K-BBQ (Thịt nướng)", "Sườn bò nướng Galbi", "Sườn bò Mỹ hảo hạng ướp sốt Galbi truyền thống đậm đà.", 329000m, 30),
            ("K-BBQ (Thịt nướng)", "Nạc vai heo sốt cay", "Nạc vai heo mềm ướp sốt BBQ cay đặc trưng Hàn Quốc.", 169000m, 40),
            ("Lẩu & Canh (Soup & Hotpot)", "Lẩu quân đội Budae-jjigae", "Lẩu cay Hàn Quốc với xúc xích, spam, kim chi, phô mai và mì gói.", 289000m, 25),
            ("Lẩu & Canh (Soup & Hotpot)", "Canh Kimchi Kimchijigae", "Canh kimchi sôi sùng sục nấu với thịt ba chỉ và đậu hũ non.", 119000m, 40),
            ("Lẩu & Canh (Soup & Hotpot)", "Canh đậu hũ non Sundubu-jigae", "Canh đậu hũ non mềm mịn, hải sản tôm mực chua cay kích thích vị giác.", 109000m, 35),
            ("Cơm & Mì (Main Rice & Noodles)", "Cơm trộn Bibimbap", "Cơm nóng với thịt bò, rau củ đa sắc và lòng đỏ trứng gà kèm sốt Gochujang.", 129000m, 45),
            ("Cơm & Mì (Main Rice & Noodles)", "Mì tương đen Jajangmyeon", "Sợi mì tươi trộn sốt tương đen đậm đà với thịt heo bằm và khoai tây.", 119000m, 40),
            ("Cơm & Mì (Main Rice & Noodles)", "Bánh gạo cay Tteokbokki", "Bánh gạo dẻo nếp dai, chả cá Odeng và nước sốt ớt cay ngọt Hàn Quốc.", 99000m, 50),
            ("Ăn kèm & Panchan (Side dishes)", "Miến trộn Japchae", "Miến khoai tây trộn dầu mè với thịt bò thăn và rau củ thanh ngọt.", 139000m, 30),
            ("Ăn kèm & Panchan (Side dishes)", "Bánh xèo Kimchi Pajeon", "Bánh xèo chiên giòn rụm với kimchi và hành lá xắt khúc.", 129000m, 35),
            ("Đồ uống & Soju (Drinks)", "Rượu Soju truyền thống Chamisul", "Thức uống quốc dân Hàn Quốc mát lạnh, ướp đá gạch vị êm dịu.", 120000m, 100),
            ("Đồ uống & Soju (Drinks)", "Rượu gạo Makgeolli", "Rượu gạo truyền thống Hàn Quốc có vị ngọt bùi và ga nhẹ dịu.", 140000m, 60),
            ("Đồ uống & Soju (Drinks)", "Trà sâm mật ong Hàn Quốc", "Trà sâm Panax tốt cho sức khỏe hòa quyện vị ngọt thanh của mật ong.", 39000m, 80),
            ("Đồ uống & Soju (Drinks)", "Nước suối", "Nước suối Aquafina tinh khiết mát lịm.", 15000m, 150)
        ];

        Dictionary<string, MenuItem> menuItems = [];

        foreach ((string categoryName, string name, string description, decimal price, int stock) in seeds)
        {
            MenuItem? item = await dbContext.MenuItems.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

            if (item is null)
            {
                item = new MenuItem
                {
                    CategoryId = categories[categoryName].CategoryId,
                    Name = name,
                    Description = description,
                    BasePrice = price,
                    ImageUrl = null,
                    IsAvailable = true,
                    Stock = stock,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await dbContext.MenuItems.AddAsync(item, cancellationToken);
            }

            menuItems[name] = item;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return menuItems;
    }

    private static async Task<Dictionary<string, ChoiceGroup>> SeedChoiceGroupsAsync(
        AppDbContext dbContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string Name, bool Required, int MaxSelect)[] seeds =
        [
            ("Mức cay", true, 1),
            ("Topping K-BBQ", false, 3),
            ("Topping Lẩu", false, 4),
            ("Size Rượu gạo", true, 1)
        ];

        Dictionary<string, ChoiceGroup> choiceGroups = [];

        foreach ((string name, bool required, int maxSelect) in seeds)
        {
            ChoiceGroup? group = await dbContext.ChoiceGroups.FirstOrDefaultAsync(x => x.GroupName == name, cancellationToken);

            if (group is null)
            {
                group = new ChoiceGroup
                {
                    GroupName = name,
                    IsAvailable = true,
                    IsRequired = required,
                    MaxSelectDefault = maxSelect,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await dbContext.ChoiceGroups.AddAsync(group, cancellationToken);
            }

            choiceGroups[name] = group;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return choiceGroups;
    }

    private static async Task<Dictionary<string, ChoiceItem>> SeedChoiceItemsAsync(
        AppDbContext dbContext,
        IReadOnlyDictionary<string, ChoiceGroup> groups,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string Group, string Name, decimal ExtraPrice)[] seeds =
        [
            ("Mức cay", "Không cay", 0m),
            ("Mức cay", "Cay vừa", 0m),
            ("Mức cay", "Rất cay", 0m),
            ("Topping K-BBQ", "Tỏi & Ớt xanh cuốn", 5000m),
            ("Topping K-BBQ", "Sốt chấm Ssamjang thêm", 5000m),
            ("Topping K-BBQ", "Rau cuốn thêm", 10000m),
            ("Topping Lẩu", "Mì gói Hàn Quốc", 15000m),
            ("Topping Lẩu", "Phô mai Mozzarella sợi", 20000m),
            ("Topping Lẩu", "Bánh gạo thêm", 15000m),
            ("Topping Lẩu", "Trứng gà tươi", 5000m),
            ("Size Rượu gạo", "Chai thường 750ml", 0m),
            ("Size Rượu gạo", "Ấm truyền thống 1.2L", 60000m)
        ];

        Dictionary<string, ChoiceItem> choiceItems = [];

        foreach ((string groupName, string name, decimal extraPrice) in seeds)
        {
            int choiceGroupId = groups[groupName].ChoiceGroupId;
            ChoiceItem? choiceItem = await dbContext.ChoiceItems.FirstOrDefaultAsync(
                x => x.ChoiceGroupId == choiceGroupId && x.ChoiceName == name,
                cancellationToken);

            if (choiceItem is null)
            {
                choiceItem = new ChoiceItem
                {
                    ChoiceGroupId = choiceGroupId,
                    ChoiceName = name,
                    ExtraPrice = extraPrice,
                    IsAvailable = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await dbContext.ChoiceItems.AddAsync(choiceItem, cancellationToken);
            }
            else
            {
                choiceItem.ExtraPrice = extraPrice;
                choiceItem.IsAvailable = true;
                choiceItem.UpdatedAt = now;
            }

            choiceItems[$"{groupName}:{name}"] = choiceItem;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return choiceItems;
    }

    private static async Task SeedMenuItemChoiceGroupsAsync(
        AppDbContext dbContext,
        IReadOnlyDictionary<string, MenuItem> menuItems,
        IReadOnlyDictionary<string, ChoiceGroup> choiceGroups,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string MenuItem, string ChoiceGroup, int DisplayOrder, int? MaxSelect)[] seeds =
        [
            ("Ba chỉ heo nướng Samgyeopsal", "Topping K-BBQ", 0, null),
            ("Sườn bò nướng Galbi", "Topping K-BBQ", 0, null),
            ("Nạc vai heo sốt cay", "Mức cay", 0, null),
            ("Nạc vai heo sốt cay", "Topping K-BBQ", 1, null),
            ("Lẩu quân đội Budae-jjigae", "Mức cay", 0, null),
            ("Lẩu quân đội Budae-jjigae", "Topping Lẩu", 1, null),
            ("Canh Kimchi Kimchijigae", "Mức cay", 0, null),
            ("Canh đậu hũ non Sundubu-jigae", "Mức cay", 0, null),
            ("Cơm trộn Bibimbap", "Mức cay", 0, null),
            ("Rượu gạo Makgeolli", "Size Rượu gạo", 0, null)
        ];

        foreach ((string menuItemName, string choiceGroupName, int displayOrder, int? maxSelect) in seeds)
        {
            if (!menuItems.TryGetValue(menuItemName, out MenuItem? menuItem) ||
                !choiceGroups.TryGetValue(choiceGroupName, out ChoiceGroup? choiceGroup))
            {
                continue;
            }

            MenuItemChoiceGroup? link = await dbContext.MenuItemChoiceGroups.FirstOrDefaultAsync(
                x => x.MenuItemId == menuItem.MenuItemId && x.ChoiceGroupId == choiceGroup.ChoiceGroupId,
                cancellationToken);

            if (link is null)
            {
                await dbContext.MenuItemChoiceGroups.AddAsync(new MenuItemChoiceGroup
                {
                    MenuItemId = menuItem.MenuItemId,
                    ChoiceGroupId = choiceGroup.ChoiceGroupId,
                    DisplayOrder = displayOrder,
                    MaxSelect = maxSelect,
                    CreatedAt = now,
                    UpdatedAt = now
                }, cancellationToken);
            }
            else
            {
                link.DisplayOrder = displayOrder;
                link.MaxSelect = maxSelect;
                link.UpdatedAt = now;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMenuItemChannelPricesAsync(
        AppDbContext dbContext,
        IReadOnlyDictionary<string, MenuItem> menuItems,
        IReadOnlyDictionary<string, SalesChannel> salesChannels,
        DateTime now,
        CancellationToken cancellationToken)
    {
        Dictionary<(string MenuItem, string Channel), decimal> extraPrices = new()
        {
            [("Ba chỉ heo nướng Samgyeopsal", "SHOPEEFOOD")] = 15000m,
            [("Ba chỉ heo nướng Samgyeopsal", "GRABFOOD")] = 20000m,
            [("Sườn bò nướng Galbi", "SHOPEEFOOD")] = 25000m,
            [("Sườn bò nướng Galbi", "GRABFOOD")] = 30000m,
            [("Lẩu quân đội Budae-jjigae", "SHOPEEFOOD")] = 20000m,
            [("Lẩu quân đội Budae-jjigae", "GRABFOOD")] = 25000m,
            [("Cơm trộn Bibimbap", "SHOPEEFOOD")] = 1000m,
            [("Cơm trộn Bibimbap", "GRABFOOD")] = 12000m,
            [("Mì tương đen Jajangmyeon", "SHOPEEFOOD")] = 10000m,
            [("Mì tương đen Jajangmyeon", "GRABFOOD")] = 12000m,
            [("Bánh gạo cay Tteokbokki", "SHOPEEFOOD")] = 8000m,
            [("Bánh gạo cay Tteokbokki", "GRABFOOD")] = 10000m
        };

        foreach (MenuItem menuItem in menuItems.Values)
        {
            foreach (SalesChannel channel in salesChannels.Values)
            {
                decimal extraPrice = extraPrices.GetValueOrDefault((menuItem.Name, channel.ChannelCode), 0m);
                await UpsertMenuItemChannelPriceAsync(dbContext, menuItem.MenuItemId, channel.SalesChannelId, extraPrice, now, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedChoiceItemChannelPricesAsync(
        AppDbContext dbContext,
        IReadOnlyDictionary<string, ChoiceItem> choiceItems,
        IReadOnlyDictionary<string, SalesChannel> salesChannels,
        DateTime now,
        CancellationToken cancellationToken)
    {
        Dictionary<(string Choice, string Channel), decimal> extraPrices = new()
        {
            [("Topping Lẩu:Phô mai Mozzarella sợi", "SHOPEEFOOD")] = 3000m,
            [("Topping Lẩu:Phô mai Mozzarella sợi", "GRABFOOD")] = 5000m,
            [("Topping Lẩu:Mì gói Hàn Quốc", "SHOPEEFOOD")] = 2000m,
            [("Topping Lẩu:Mì gói Hàn Quốc", "GRABFOOD")] = 3000m,
            [("Size Rượu gạo:Ấm truyền thống 1.2L", "SHOPEEFOOD")] = 10000m,
            [("Size Rượu gạo:Ấm truyền thống 1.2L", "GRABFOOD")] = 15000m
        };

        foreach ((string key, ChoiceItem choiceItem) in choiceItems)
        {
            foreach (SalesChannel channel in salesChannels.Values)
            {
                decimal extraPrice = extraPrices.GetValueOrDefault((key, channel.ChannelCode), 0m);
                await UpsertChoiceItemChannelPriceAsync(dbContext, choiceItem.ChoiceItemId, channel.SalesChannelId, extraPrice, now, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task UpsertMenuItemChannelPriceAsync(
        AppDbContext dbContext,
        int menuItemId,
        int salesChannelId,
        decimal extraPrice,
        DateTime now,
        CancellationToken cancellationToken)
    {
        MenuItemChannelPrice? channelPrice = await dbContext.MenuItemChannelPrices.FirstOrDefaultAsync(
            x => x.MenuItemId == menuItemId && x.SalesChannelId == salesChannelId,
            cancellationToken);

        if (channelPrice is null)
        {
            await dbContext.MenuItemChannelPrices.AddAsync(new MenuItemChannelPrice
            {
                MenuItemId = menuItemId,
                SalesChannelId = salesChannelId,
                ChannelExtraPrice = extraPrice,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);

            return;
        }

        channelPrice.ChannelExtraPrice = extraPrice;
        channelPrice.UpdatedAt = now;
    }

    private static async Task UpsertChoiceItemChannelPriceAsync(
        AppDbContext dbContext,
        int choiceItemId,
        int salesChannelId,
        decimal extraPrice,
        DateTime now,
        CancellationToken cancellationToken)
    {
        ChoiceItemChannelPrice? channelPrice = await dbContext.ChoiceItemChannelPrices.FirstOrDefaultAsync(
            x => x.ChoiceItemId == choiceItemId && x.SalesChannelId == salesChannelId,
            cancellationToken);

        if (channelPrice is null)
        {
            await dbContext.ChoiceItemChannelPrices.AddAsync(new ChoiceItemChannelPrice
            {
                ChoiceItemId = choiceItemId,
                SalesChannelId = salesChannelId,
                ChannelExtraPrice = extraPrice,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);

            return;
        }

        channelPrice.ChannelExtraPrice = extraPrice;
        channelPrice.UpdatedAt = now;
    }

    private static async Task SeedHistoricalDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var random = new Random(42);
        var tables = await dbContext.DiningTables.ToListAsync(cancellationToken);
        var menuItems = await dbContext.MenuItems.ToListAsync(cancellationToken);
        var users = await dbContext.Users.ToListAsync(cancellationToken);
        var salesChannels = await dbContext.SalesChannels.ToListAsync(cancellationToken);

        if (tables.Count == 0 || menuItems.Count == 0 || users.Count == 0 || salesChannels.Count == 0)
        {
            Console.WriteLine("Warning: Seeding cancelled - prerequisite tables are empty.");
            return;
        }

        var channel = salesChannels.FirstOrDefault(x => x.ChannelCode == "DINE_IN") ?? salesChannels[0];
        var staffList = users.Where(x => x.Role == "Staff").ToList();
        if (staffList.Count == 0) staffList = users;

        var reasons = new[]
        {
            "Khách đổi ý muốn gọi món khác",
            "Món chuẩn bị quá lâu",
            "Nhân viên ghi nhầm món",
            "Hết nguyên liệu chế biến",
            "Khách hủy bàn đi về"
        };

        for (int dayOffset = 30; dayOffset >= 0; dayOffset--)
        {
            DateTime localDate = DateTime.Today.AddDays(-dayOffset);
            int count = random.Next(8, 16);
            
            for (int i = 0; i < count; i++)
            {
                var table = tables[random.Next(tables.Count)];
                var staff = staffList[random.Next(staffList.Count)];

                DateTime sessionStartLocal = localDate.AddHours(random.Next(10, 22)).AddMinutes(random.Next(0, 60));
                DateTime sessionStartUtc = DateTime.SpecifyKind(sessionStartLocal.AddHours(-7), DateTimeKind.Utc);
                DateTime sessionEndUtc = DateTime.SpecifyKind(sessionStartUtc.AddMinutes(random.Next(45, 95)), DateTimeKind.Utc);

                var session = new TableSession
                {
                    TableId = table.TableId,
                    StartedAt = sessionStartUtc,
                    EndedAt = sessionEndUtc,
                    Status = "Closed",
                    OpenedBy = staff.UserId,
                    ClosedBy = staff.UserId
                };
                await dbContext.TableSessions.AddAsync(session, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                int orderCount = random.Next(1, 3);
                var activeOrderItems = new List<OrderItem>();

                for (int o = 0; o < orderCount; o++)
                {
                    bool isCancelledOrder = random.NextDouble() < 0.05;
                    DateTime orderCreated = DateTime.SpecifyKind(sessionStartUtc.AddMinutes(o * 15), DateTimeKind.Utc);
                    DateTime orderUpdated = DateTime.SpecifyKind(sessionStartUtc.AddMinutes(o * 15 + 10), DateTimeKind.Utc);

                    var order = new Order
                    {
                        TableSessionId = session.TableSessionId,
                        SalesChannelId = channel.SalesChannelId,
                        OrderCode = $"ORD-{sessionStartLocal:yyyyMMdd}-{i:D3}-{o:D2}-{random.Next(1000, 9999)}",
                        OrderSource = "Staff",
                        Status = isCancelledOrder ? "Cancelled" : "Served",
                        CreatedAt = orderCreated,
                        UpdatedAt = orderUpdated,
                        CreatedBy = staff.UserId
                    };

                    if (isCancelledOrder)
                    {
                        order.CancelledAt = DateTime.SpecifyKind(orderCreated.AddMinutes(15), DateTimeKind.Utc);
                        order.CancelledBy = staff.UserId;
                        order.CancelReason = reasons[random.Next(reasons.Length)];
                    }

                    await dbContext.Orders.AddAsync(order, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    int itemCount = random.Next(2, 6);
                    for (int itemIdx = 0; itemIdx < itemCount; itemIdx++)
                    {
                        var menuItem = menuItems[random.Next(menuItems.Count)];
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            MenuItemId = menuItem.MenuItemId,
                            MenuItemNameSnapshot = menuItem.Name,
                            BasePriceSnapshot = menuItem.BasePrice,
                            ChannelExtraPriceSnapshot = 0m,
                            FinalUnitPriceSnapshot = menuItem.BasePrice,
                            Quantity = random.Next(1, 4),
                            CreatedAt = orderCreated,
                            UpdatedAt = orderUpdated
                        };

                        await dbContext.OrderItems.AddAsync(orderItem, cancellationToken);
                        if (!isCancelledOrder)
                        {
                            activeOrderItems.Add(orderItem);
                        }
                    }
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                if (activeOrderItems.Count == 0) continue;

                bool isCancelledBill = random.NextDouble() < 0.08;
                decimal subtotal = activeOrderItems.Sum(x => x.FinalUnitPriceSnapshot * x.Quantity);
                decimal discount = random.NextDouble() < 0.15 ? 10000m * random.Next(1, 5) : 0m;
                if (discount > subtotal) discount = 0m;
                decimal finalAmount = subtotal - discount;

                var bill = new Bill
                {
                    TableSessionId = session.TableSessionId,
                    SalesChannelId = channel.SalesChannelId,
                    SalesChannelCodeSnapshot = channel.ChannelCode,
                    SalesChannelNameSnapshot = channel.ChannelName,
                    BillCode = $"BILL-{sessionStartLocal:yyyyMMdd}-{i:D3}-{random.Next(1000, 9999)}",
                    BillName = $"Hóa đơn {table.TableName}",
                    BillNo = i + 1,
                    Status = isCancelledBill ? "Cancelled" : "Paid",
                    SubTotal = subtotal,
                    DiscountAmount = discount,
                    FinalAmount = finalAmount,
                    CreatedAt = sessionEndUtc,
                    CreatedBy = staff.UserId
                };

                if (isCancelledBill)
                {
                    bill.CancelledAt = DateTime.SpecifyKind(sessionEndUtc.AddMinutes(5), DateTimeKind.Utc);
                    bill.CancelledBy = staff.UserId;
                    bill.CancelReason = reasons[random.Next(reasons.Length)];
                }
                else
                {
                    bill.PaidAt = sessionEndUtc;
                }

                await dbContext.Bills.AddAsync(bill, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                var billDetails = new List<BillDetail>();
                foreach (var orderItem in activeOrderItems)
                {
                    var detail = new BillDetail
                    {
                        BillId = bill.BillId,
                        MenuItemId = orderItem.MenuItemId,
                        SalesChannelId = channel.SalesChannelId,
                        ItemName = orderItem.MenuItemNameSnapshot,
                        Quantity = orderItem.Quantity,
                        NotifiedQuantity = orderItem.Quantity,
                        BasePriceSnapshot = orderItem.BasePriceSnapshot,
                        MenuItemChannelExtraPriceSnapshot = 0m,
                        ChoiceExtraPriceSnapshot = 0m,
                        UnitPrice = orderItem.FinalUnitPriceSnapshot,
                        TotalPrice = orderItem.FinalUnitPriceSnapshot * orderItem.Quantity,
                        CreatedAt = sessionEndUtc
                    };
                    await dbContext.BillDetails.AddAsync(detail, cancellationToken);
                    billDetails.Add(detail);
                }
                await dbContext.SaveChangesAsync(cancellationToken);

                if (!isCancelledBill)
                {
                    bool isCombined = random.NextDouble() < 0.25;
                    if (isCombined)
                    {
                        string firstMethod = "Cash";
                        string secondMethod = random.NextDouble() < 0.5 ? "Card" : "BankTransfer";

                        decimal firstAmount = Math.Round((finalAmount * (decimal)(0.3 + random.NextDouble() * 0.4)) / 1000m) * 1000m;
                        if (firstAmount >= finalAmount) firstAmount = finalAmount - 10000m;
                        if (firstAmount <= 0) firstAmount = 10000m;
                        decimal secondAmount = finalAmount - firstAmount;

                        var payment1 = new Payment
                        {
                            BillId = bill.BillId,
                            PaymentMethod = firstMethod,
                            Amount = firstAmount,
                            PaidAt = sessionEndUtc,
                            ConfirmedBy = staff.UserId
                        };

                        var payment2 = new Payment
                        {
                            BillId = bill.BillId,
                            PaymentMethod = secondMethod,
                            Amount = secondAmount,
                            PaidAt = sessionEndUtc,
                            ConfirmedBy = staff.UserId
                        };

                        await dbContext.Payments.AddAsync(payment1, cancellationToken);
                        await dbContext.Payments.AddAsync(payment2, cancellationToken);
                    }
                    else
                    {
                        var methods = new[] { "Cash", "BankTransfer", "Card" };
                        string method = methods[random.Next(methods.Length)];

                        var payment = new Payment
                        {
                            BillId = bill.BillId,
                            PaymentMethod = method,
                            Amount = finalAmount,
                            PaidAt = sessionEndUtc,
                            ConfirmedBy = staff.UserId
                        };
                        await dbContext.Payments.AddAsync(payment, cancellationToken);
                    }

                    bool hasAdjustment = random.NextDouble() < 0.15;
                    if (hasAdjustment && billDetails.Count > 0)
                    {
                        var targetDetail = billDetails[random.Next(billDetails.Count)];
                        int qtyChange = random.Next(1, targetDetail.Quantity + 1);
                        string changeType = qtyChange == targetDetail.Quantity ? "CancelItem" : "ReduceQuantity";

                        var adj = new BillDetailAdjustment
                        {
                            BillId = bill.BillId,
                            BillDetailId = targetDetail.BillDetailId,
                            MenuItemId = targetDetail.MenuItemId,
                            ItemName = targetDetail.ItemName,
                            QuantityBefore = targetDetail.Quantity,
                            QuantityAfter = targetDetail.Quantity - qtyChange,
                            ChangedQuantity = qtyChange,
                            ChangeType = changeType,
                            Reason = reasons[random.Next(reasons.Length)],
                            CreatedAt = DateTime.SpecifyKind(sessionEndUtc.AddMinutes(-5), DateTimeKind.Utc),
                            CreatedBy = staff.UserId
                        };
                        await dbContext.BillDetailAdjustments.AddAsync(adj, cancellationToken);
                    }
                }
                
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        Console.WriteLine("Historical billing and cancellation data generated successfully.");
    }
}
