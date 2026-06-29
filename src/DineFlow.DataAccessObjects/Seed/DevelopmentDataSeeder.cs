using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace DineFlow.DataAccessObjects.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedDevelopmentDataAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (!await HasCurrentSchemaAsync(dbContext, cancellationToken))
        {
            Console.WriteLine("Development seed skipped: database schema is not current. Recreate/update the database, then start the API again.");
            return;
        }

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

        _ = users;
        _ = tables;
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
            ("DINE_IN", "Tai quan"),
            ("CUSTOMER_WEB", "Khach quet QR"),
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
        User admin = await GetOrCreateUserAsync(
            dbContext,
            username: "admin",
            passwordHash: "admin123",
            fullName: "Quan tri vien",
            role: "Admin",
            now,
            cancellationToken);

        User staff = await GetOrCreateUserAsync(
            dbContext,
            username: "staff01",
            passwordHash: "staff123",
            fullName: "Nhan vien 01",
            role: "Staff",
            now,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new Dictionary<string, User>
        {
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
        (string Name, string Area, string Token)[] seeds =
        [
            ("Ban 01", "Tang 1", "QR-TABLE-001"),
            ("Ban 02", "Tang 1", "QR-TABLE-002"),
            ("Ban 03", "Tang 1", "QR-TABLE-003"),
            ("Ban 04", "Tang 2", "QR-TABLE-004"),
            ("Ban 05", "Tang 2", "QR-TABLE-005"),
            ("Ban VIP 01", "VIP", "QR-VIP-001")
        ];

        Dictionary<string, DiningTable> tables = [];

        foreach ((string name, string area, string token) in seeds)
        {
            DiningTable? table = await dbContext.DiningTables.FirstOrDefaultAsync(x => x.QrToken == token, cancellationToken);

            if (table is null)
            {
                table = new DiningTable
                {
                    TableName = name,
                    Area = area,
                    QrToken = token,
                    Status = "Available",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await dbContext.DiningTables.AddAsync(table, cancellationToken);
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
            ("Mon chinh", "Cac mon an no", 1),
            ("Do uong", "Nuoc, tra, ca phe", 2),
            ("Mon them", "Topping, an kem", 3)
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
            ("Mon chinh", "Com ga xoi mo", "Com ga chien gion, mo hanh, nuoc mam chua ngot", 55000m, 30),
            ("Mon chinh", "Mi bo cay", "Mi bo voi nuoc dung cay vua", 65000m, 25),
            ("Mon chinh", "Bun thit nuong", "Bun, thit nuong, rau song, nuoc mam", 50000m, 30),
            ("Mon chinh", "Com suon trung", "Com tam suon nuong va trung op la", 60000m, 20),
            ("Do uong", "Tra dao", "Tra dao cam sa mat lanh", 30000m, 40),
            ("Do uong", "Tra sua truyen thong", "Tra sua vi truyen thong", 35000m, 40),
            ("Do uong", "Ca phe sua", "Ca phe sua da Viet Nam", 28000m, 50),
            ("Do uong", "Nuoc suoi", "Nuoc suoi dong chai", 10000m, 100),
            ("Mon them", "Khoai tay chien", "Khoai tay chien gion", 35000m, 20),
            ("Mon them", "Salad nho", "Salad rau cu phan nho", 25000m, 15)
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
            ("Size", true, 1),
            ("Duong", true, 1),
            ("Da", true, 1),
            ("Topping", false, 3),
            ("Muc cay", true, 1)
        ];

        Dictionary<string, ChoiceGroup> groups = [];

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

            groups[name] = group;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return groups;
    }

    private static async Task<Dictionary<string, ChoiceItem>> SeedChoiceItemsAsync(
        AppDbContext dbContext,
        IReadOnlyDictionary<string, ChoiceGroup> groups,
        DateTime now,
        CancellationToken cancellationToken)
    {
        (string Group, string Name, decimal ExtraPrice)[] seeds =
        [
            ("Size", "M", 0m),
            ("Size", "L", 7000m),
            ("Size", "XL", 12000m),
            ("Duong", "0% duong", 0m),
            ("Duong", "50% duong", 0m),
            ("Duong", "100% duong", 0m),
            ("Da", "Khong da", 0m),
            ("Da", "It da", 0m),
            ("Da", "Binh thuong", 0m),
            ("Topping", "Tran chau den", 7000m),
            ("Topping", "Thach ca phe", 6000m),
            ("Topping", "Pudding trung", 8000m),
            ("Muc cay", "Khong cay", 0m),
            ("Muc cay", "Cay vua", 0m),
            ("Muc cay", "Rat cay", 0m)
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
            ("Tra dao", "Size", 1, null),
            ("Tra dao", "Duong", 2, null),
            ("Tra dao", "Da", 3, null),
            ("Tra dao", "Topping", 4, 2),
            ("Tra sua truyen thong", "Size", 1, null),
            ("Tra sua truyen thong", "Duong", 2, null),
            ("Tra sua truyen thong", "Da", 3, null),
            ("Tra sua truyen thong", "Topping", 4, 3),
            ("Ca phe sua", "Duong", 1, null),
            ("Ca phe sua", "Da", 2, null),
            ("Mi bo cay", "Muc cay", 1, null)
        ];

        foreach ((string menuItemName, string choiceGroupName, int displayOrder, int? maxSelect) in seeds)
        {
            int menuItemId = menuItems[menuItemName].MenuItemId;
            int choiceGroupId = choiceGroups[choiceGroupName].ChoiceGroupId;

            MenuItemChoiceGroup? assignment = await dbContext.MenuItemChoiceGroups.FirstOrDefaultAsync(
                x => x.MenuItemId == menuItemId && x.ChoiceGroupId == choiceGroupId,
                cancellationToken);

            if (assignment is not null)
            {
                assignment.DisplayOrder = displayOrder;
                assignment.MaxSelect = maxSelect;
                assignment.UpdatedAt = now;
                continue;
            }

            await dbContext.MenuItemChoiceGroups.AddAsync(new MenuItemChoiceGroup
            {
                MenuItemId = menuItemId,
                ChoiceGroupId = choiceGroupId,
                DisplayOrder = displayOrder,
                MaxSelect = maxSelect,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
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
            [("Com ga xoi mo", "SHOPEEFOOD")] = 8000m,
            [("Com ga xoi mo", "GRABFOOD")] = 10000m,
            [("Mi bo cay", "SHOPEEFOOD")] = 9000m,
            [("Mi bo cay", "GRABFOOD")] = 11000m,
            [("Bun thit nuong", "SHOPEEFOOD")] = 7000m,
            [("Bun thit nuong", "GRABFOOD")] = 9000m,
            [("Com suon trung", "SHOPEEFOOD")] = 9000m,
            [("Com suon trung", "GRABFOOD")] = 11000m,
            [("Tra dao", "SHOPEEFOOD")] = 5000m,
            [("Tra dao", "GRABFOOD")] = 6000m,
            [("Tra sua truyen thong", "SHOPEEFOOD")] = 5000m,
            [("Tra sua truyen thong", "GRABFOOD")] = 7000m,
            [("Ca phe sua", "SHOPEEFOOD")] = 4000m,
            [("Ca phe sua", "GRABFOOD")] = 5000m,
            [("Nuoc suoi", "SHOPEEFOOD")] = 2000m,
            [("Nuoc suoi", "GRABFOOD")] = 3000m,
            [("Khoai tay chien", "SHOPEEFOOD")] = 5000m,
            [("Khoai tay chien", "GRABFOOD")] = 6000m,
            [("Salad nho", "SHOPEEFOOD")] = 4000m,
            [("Salad nho", "GRABFOOD")] = 5000m
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
            [("Size:L", "SHOPEEFOOD")] = 1000m,
            [("Size:L", "GRABFOOD")] = 2000m,
            [("Size:XL", "SHOPEEFOOD")] = 2000m,
            [("Size:XL", "GRABFOOD")] = 3000m,
            [("Topping:Tran chau den", "SHOPEEFOOD")] = 1000m,
            [("Topping:Tran chau den", "GRABFOOD")] = 2000m,
            [("Topping:Thach ca phe", "SHOPEEFOOD")] = 1000m,
            [("Topping:Thach ca phe", "GRABFOOD")] = 2000m,
            [("Topping:Pudding trung", "SHOPEEFOOD")] = 2000m,
            [("Topping:Pudding trung", "GRABFOOD")] = 3000m
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
}
