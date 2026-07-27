-- Idempotent PostgreSQL Seeder for DineFlow Korean Menu Expansion
-- Safe to execute multiple times against Neon DB

DO $$
DECLARE
    v_now TIMESTAMP WITH TIME ZONE := NOW();
    
    -- Category IDs
    v_cat_bbq INT;
    v_cat_lau INT;
    v_cat_com INT;
    v_cat_side INT;
    v_cat_drink INT;
    
    -- Channel IDs
    v_chan_dinein INT;
    v_chan_web INT;
    v_chan_shopee INT;
    v_chan_grab INT;

    -- Choice Group IDs
    v_group_spicy INT;
    v_group_bbq_top INT;
    v_group_lau_top INT;
    v_group_soju_size INT;
    v_group_bbq_sauce INT;
    v_group_portion_size INT;
    v_group_rice_top INT;
    v_group_drink_ice INT;
    v_group_panchan INT;

    -- Temporary helpers
    v_item_id INT;
    v_choice_id INT;
BEGIN
    RAISE NOTICE 'Starting DineFlow Korean Menu Seeder...';

    ----------------------------------------------------------------------------
    -- 1. SEED / FETCH SALES CHANNELS
    ----------------------------------------------------------------------------
    SELECT "SalesChannelId" INTO v_chan_dinein FROM "SalesChannels" WHERE "ChannelCode" = 'DINE_IN';
    IF v_chan_dinein IS NULL THEN
        INSERT INTO "SalesChannels" ("ChannelCode", "ChannelName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
        VALUES ('DINE_IN', 'Kênh bán tại quán', true, false, v_now, v_now) RETURNING "SalesChannelId" INTO v_chan_dinein;
    END IF;

    SELECT "SalesChannelId" INTO v_chan_web FROM "SalesChannels" WHERE "ChannelCode" = 'CUSTOMER_WEB';
    IF v_chan_web IS NULL THEN
        INSERT INTO "SalesChannels" ("ChannelCode", "ChannelName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
        VALUES ('CUSTOMER_WEB', 'Khách quét QR', true, false, v_now, v_now) RETURNING "SalesChannelId" INTO v_chan_web;
    END IF;

    SELECT "SalesChannelId" INTO v_chan_shopee FROM "SalesChannels" WHERE "ChannelCode" = 'SHOPEEFOOD';
    IF v_chan_shopee IS NULL THEN
        INSERT INTO "SalesChannels" ("ChannelCode", "ChannelName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
        VALUES ('SHOPEEFOOD', 'ShopeeFood', true, false, v_now, v_now) RETURNING "SalesChannelId" INTO v_chan_shopee;
    END IF;

    SELECT "SalesChannelId" INTO v_chan_grab FROM "SalesChannels" WHERE "ChannelCode" = 'GRABFOOD';
    IF v_chan_grab IS NULL THEN
        INSERT INTO "SalesChannels" ("ChannelCode", "ChannelName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
        VALUES ('GRABFOOD', 'GrabFood', true, false, v_now, v_now) RETURNING "SalesChannelId" INTO v_chan_grab;
    END IF;

    ----------------------------------------------------------------------------
    -- 2. SEED / FETCH CATEGORIES
    ----------------------------------------------------------------------------
    SELECT "CategoryId" INTO v_cat_bbq FROM "Categories" WHERE "CategoryName" LIKE 'K-BBQ%';
    IF v_cat_bbq IS NULL THEN
        INSERT INTO "Categories" ("CategoryName", "Description", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES ('K-BBQ (Thịt nướng)', 'Thịt bò và thịt heo nướng hảo hạng', 1, true, v_now, v_now)
        RETURNING "CategoryId" INTO v_cat_bbq;
    END IF;

    SELECT "CategoryId" INTO v_cat_lau FROM "Categories" WHERE "CategoryName" LIKE 'Lẩu & Canh%';
    IF v_cat_lau IS NULL THEN
        INSERT INTO "Categories" ("CategoryName", "Description", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES ('Lẩu & Canh (Soup & Hotpot)', 'Canh truyền thống và lẩu Hàn Quốc ấm nóng', 2, true, v_now, v_now)
        RETURNING "CategoryId" INTO v_cat_lau;
    END IF;

    SELECT "CategoryId" INTO v_cat_com FROM "Categories" WHERE "CategoryName" LIKE 'Cơm & Mì%';
    IF v_cat_com IS NULL THEN
        INSERT INTO "Categories" ("CategoryName", "Description", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES ('Cơm & Mì (Main Rice & Noodles)', 'Cơm trộn và mì đặc trưng xứ Hàn', 3, true, v_now, v_now)
        RETURNING "CategoryId" INTO v_cat_com;
    END IF;

    SELECT "CategoryId" INTO v_cat_side FROM "Categories" WHERE "CategoryName" LIKE 'Ăn kèm & Panchan%';
    IF v_cat_side IS NULL THEN
        INSERT INTO "Categories" ("CategoryName", "Description", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES ('Ăn kèm & Panchan (Side dishes)', 'Panchan và món khai vị ăn kèm hấp dẫn', 4, true, v_now, v_now)
        RETURNING "CategoryId" INTO v_cat_side;
    END IF;

    SELECT "CategoryId" INTO v_cat_drink FROM "Categories" WHERE "CategoryName" LIKE 'Đồ uống & Soju%';
    IF v_cat_drink IS NULL THEN
        INSERT INTO "Categories" ("CategoryName", "Description", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt")
        VALUES ('Đồ uống & Soju (Drinks)', 'Rượu Soju, rượu gạo truyền thống và nước giải khát', 5, true, v_now, v_now)
        RETURNING "CategoryId" INTO v_cat_drink;
    END IF;

    ----------------------------------------------------------------------------
    -- 3. SEED / FETCH CHOICE GROUPS & ITEMS
    ----------------------------------------------------------------------------
    -- Mức cay
    SELECT "ChoiceGroupId" INTO v_group_spicy FROM "ChoiceGroups" WHERE "GroupName" = 'Mức cay';
    IF v_group_spicy IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Mức cay', true, true, 1, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_spicy;
    END IF;

    -- Topping K-BBQ
    SELECT "ChoiceGroupId" INTO v_group_bbq_top FROM "ChoiceGroups" WHERE "GroupName" = 'Topping K-BBQ';
    IF v_group_bbq_top IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Topping K-BBQ', true, false, 3, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_bbq_top;
    END IF;

    -- Topping Lẩu
    SELECT "ChoiceGroupId" INTO v_group_lau_top FROM "ChoiceGroups" WHERE "GroupName" = 'Topping Lẩu';
    IF v_group_lau_top IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Topping Lẩu', true, false, 4, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_lau_top;
    END IF;

    -- Size Rượu gạo
    SELECT "ChoiceGroupId" INTO v_group_soju_size FROM "ChoiceGroups" WHERE "GroupName" = 'Size Rượu gạo';
    IF v_group_soju_size IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Size Rượu gạo', true, true, 1, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_soju_size;
    END IF;

    -- NEW: Sốt chấm BBQ
    SELECT "ChoiceGroupId" INTO v_group_bbq_sauce FROM "ChoiceGroups" WHERE "GroupName" = 'Sốt chấm BBQ';
    IF v_group_bbq_sauce IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Sốt chấm BBQ', true, false, 2, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_bbq_sauce;
    END IF;

    -- NEW: Size phần ăn
    SELECT "ChoiceGroupId" INTO v_group_portion_size FROM "ChoiceGroups" WHERE "GroupName" = 'Size phần ăn';
    IF v_group_portion_size IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Size phần ăn', true, true, 1, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_portion_size;
    END IF;

    -- NEW: Topping cơm mì
    SELECT "ChoiceGroupId" INTO v_group_rice_top FROM "ChoiceGroups" WHERE "GroupName" = 'Topping cơm mì';
    IF v_group_rice_top IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Topping cơm mì', true, false, 3, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_rice_top;
    END IF;

    -- NEW: Đồ uống (đá)
    SELECT "ChoiceGroupId" INTO v_group_drink_ice FROM "ChoiceGroups" WHERE "GroupName" = 'Đồ uống';
    IF v_group_drink_ice IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Đồ uống', true, true, 1, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_drink_ice;
    END IF;

    -- NEW: Combo Panchan
    SELECT "ChoiceGroupId" INTO v_group_panchan FROM "ChoiceGroups" WHERE "GroupName" = 'Combo Panchan';
    IF v_group_panchan IS NULL THEN
        INSERT INTO "ChoiceGroups" ("GroupName", "IsAvailable", "IsRequired", "MaxSelectDefault", "CreatedAt", "UpdatedAt")
        VALUES ('Combo Panchan', true, false, 4, v_now, v_now) RETURNING "ChoiceGroupId" INTO v_group_panchan;
    END IF;

    ----------------------------------------------------------------------------
    -- CHOICE ITEMS SEEDING
    ----------------------------------------------------------------------------
    -- Sốt chấm BBQ
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_bbq_sauce AND "ChoiceName" = 'Ssamjang';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_bbq_sauce, 'Ssamjang', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_bbq_sauce AND "ChoiceName" = 'Gochujang';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_bbq_sauce, 'Gochujang', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_bbq_sauce AND "ChoiceName" = 'Dầu mè muối';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_bbq_sauce, 'Dầu mè muối', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_bbq_sauce AND "ChoiceName" = 'Sốt tiêu đen';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_bbq_sauce, 'Sốt tiêu đen', 0, true, v_now, v_now); END IF;

    -- Size phần ăn
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_portion_size AND "ChoiceName" = 'Vừa';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_portion_size, 'Vừa', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_portion_size AND "ChoiceName" = 'Lớn';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_portion_size, 'Lớn', 30000, true, v_now, v_now); END IF;

    -- Topping cơm mì
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_rice_top AND "ChoiceName" = 'Trứng lòng đào';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_rice_top, 'Trứng lòng đào', 10000, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_rice_top AND "ChoiceName" = 'Phô mai';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_rice_top, 'Phô mai', 15000, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_rice_top AND "ChoiceName" = 'Kimchi thêm';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_rice_top, 'Kimchi thêm', 10000, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_rice_top AND "ChoiceName" = 'Rong biển';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_rice_top, 'Rong biển', 8000, true, v_now, v_now); END IF;

    -- Đồ uống
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_drink_ice AND "ChoiceName" = 'Bình thường';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_drink_ice, 'Bình thường', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_drink_ice AND "ChoiceName" = 'Ít đá';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_drink_ice, 'Ít đá', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_drink_ice AND "ChoiceName" = 'Không đá';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_drink_ice, 'Không đá', 0, true, v_now, v_now); END IF;

    -- Combo Panchan
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_panchan AND "ChoiceName" = 'Kimchi cải thảo';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_panchan, 'Kimchi cải thảo', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_panchan AND "ChoiceName" = 'Củ cải muối';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_panchan, 'Củ cải muối', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_panchan AND "ChoiceName" = 'Giá đỗ';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_panchan, 'Giá đỗ', 0, true, v_now, v_now); END IF;
    PERFORM 1 FROM "ChoiceItems" WHERE "ChoiceGroupId" = v_group_panchan AND "ChoiceName" = 'Rong biển';
    IF NOT FOUND THEN INSERT INTO "ChoiceItems" ("ChoiceGroupId", "ChoiceName", "ExtraPrice", "IsAvailable", "CreatedAt", "UpdatedAt") VALUES (v_group_panchan, 'Rong biển', 0, true, v_now, v_now); END IF;

    ----------------------------------------------------------------------------
    -- 4. MENU ITEMS & CHANNEL PRICES SEEDING
    ----------------------------------------------------------------------------
    -- Item 1: Bò ba chỉ Mỹ nướng
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Bò ba chỉ Mỹ nướng';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_bbq, 'Bò ba chỉ Mỹ nướng', 'Thịt ba chỉ bò Mỹ tươi ngon thái lát mỏng, nướng giòn thơm ngậy ăn kèm sốt nướng đặc trưng.', 199000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/bo-ba-chi-my-nuong.jpg', true, false, false, 40, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 199000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/bo-ba-chi-my-nuong.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_sauce) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_sauce, 2, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_top, 3, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 15000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 18000, v_now, v_now);
    END IF;

    -- Item 2: Thăn bò sốt Bulgogi
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Thăn bò sốt Bulgogi';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_bbq, 'Thăn bò sốt Bulgogi', 'Thăn bò tươi ướp sốt Bulgogi hoa quả truyền thống thơm ngọt đậm đà.', 249000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/than-bo-sot-bulgogi.jpg', true, false, false, 30, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 249000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/than-bo-sot-bulgogi.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_sauce) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_sauce, 2, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_top, 3, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 20000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 25000, v_now, v_now);
    END IF;

    -- Item 3: Gà nướng sốt mật ong
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Gà nướng sốt mật ong';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_bbq, 'Gà nướng sốt mật ong', 'Đùi gà rút xương ướp sốt mật ong mù tạt nướng xém cạnh da giòn rụm.', 169000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/ga-nuong-sot-mat-ong.jpg', true, false, false, 35, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 169000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/ga-nuong-sot-mat-ong.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_sauce) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_sauce, 2, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_top, 3, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 12000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 15000, v_now, v_now);
    END IF;

    -- Item 4: Bạch tuộc nướng cay
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Bạch tuộc nướng cay';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_bbq, 'Bạch tuộc nướng cay', 'Bạch tuộc tươi giòn sần sật ướp sốt Gochujang siêu cay nướng than hồng.', 219000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/bach-tuoc-nuong-cay.jpg', true, false, false, 25, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 219000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/bach-tuoc-nuong-cay.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_spicy) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_spicy, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_sauce) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_sauce, 2, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_bbq_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_bbq_top, 3, 3, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 18000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 22000, v_now, v_now);
    END IF;

    -- Item 5: Lẩu kimchi hải sản
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Lẩu kimchi hải sản';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_lau, 'Lẩu kimchi hải sản', 'Lẩu kimchi đậm vị chua cay kèm tôm, mực, ngao, đậu hũ non và rau nấm tươi.', 329000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/lau-kimchi-hai-san.jpg', true, false, false, 20, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 329000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/lau-kimchi-hai-san.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_portion_size) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_portion_size, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_spicy) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_spicy, 1, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_lau_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_lau_top, 4, 3, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 25000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 30000, v_now, v_now);
    END IF;

    -- Item 6: Canh rong biển bò
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Canh rong biển bò';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_lau, 'Canh rong biển bò', 'Canh rong biển Miyeok-guk nấu cùng thịt bò tươi mang lại vị ngọt thanh bổ dưỡng.', 119000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/canh-rong-bien-bo.jpg', true, false, false, 40, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 119000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/canh-rong-bien-bo.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_portion_size) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_portion_size, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 10000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 12000, v_now, v_now);
    END IF;

    -- Item 7: Canh tương đậu Doenjang
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Canh tương đậu Doenjang';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_lau, 'Canh tương đậu Doenjang', 'Canh tương đậu đậu hũ truyền thống chuẩn vị nhà làm thanh mát thơm bùi.', 129000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/canh-tuong-dau-doenjang.jpg', true, false, false, 35, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 129000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/canh-tuong-dau-doenjang.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_portion_size) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_portion_size, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_spicy) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_spicy, 1, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 10000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 13000, v_now, v_now);
    END IF;

    -- Item 8: Lẩu nấm bò Bulgogi
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Lẩu nấm bò Bulgogi';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_lau, 'Lẩu nấm bò Bulgogi', 'Lẩu nấm tổng hợp thượng hạng kết hợp thịt bò Bulgogi xào sốt ngọt thơm thanh.', 349000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/lau-nam-bo-bulgogi.jpg', true, false, false, 20, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 349000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/lau-nam-bo-bulgogi.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_portion_size) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_portion_size, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_lau_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_lau_top, 4, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 25000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 30000, v_now, v_now);
    END IF;

    -- Item 9: Cơm chiên kimchi
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Cơm chiên kimchi';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_com, 'Cơm chiên kimchi', 'Cơm chiên kimchi chua cay thơm lừng phủ trứng chiên lòng đào và rong biển vụn.', 89000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/com-chien-kimchi.jpg', true, false, false, 50, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 89000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/com-chien-kimchi.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_spicy) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_spicy, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_rice_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_rice_top, 3, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 8000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 10000, v_now, v_now);
    END IF;

    -- Item 10: Mì lạnh Naengmyeon
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Mì lạnh Naengmyeon';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_com, 'Mì lạnh Naengmyeon', 'Mì lạnh sợi dai giòn tan trong nước dùng đá bào chua ngọt mát lạnh giải nhiệt.', 109000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/mi-lanh-naengmyeon.jpg', true, false, false, 30, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 109000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/mi-lanh-naengmyeon.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_rice_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_rice_top, 3, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 10000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 12000, v_now, v_now);
    END IF;

    -- Item 11: Kimbap bò Bulgogi
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Kimbap bò Bulgogi';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_com, 'Kimbap bò Bulgogi', 'Cơm cuộn lá bàng nhân thịt bò Bulgogi đậm đà, trứng chiên và củ cải muối.', 79000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/kimbap-bo-bulgogi.jpg', true, false, false, 45, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 79000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/kimbap-bo-bulgogi.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_panchan) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_panchan, 4, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 8000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 10000, v_now, v_now);
    END IF;

    -- Item 12: Cơm gà sốt Gochujang
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Cơm gà sốt Gochujang';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_com, 'Cơm gà sốt Gochujang', 'Cơm trắng nóng hổi ăn kèm gà sốt Gochujang cay ngọt đậm đà hấp dẫn.', 99000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/com-ga-sot-gochujang.jpg', true, false, false, 40, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 99000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/com-ga-sot-gochujang.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_spicy) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_spicy, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_rice_top) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_rice_top, 3, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 9000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 11000, v_now, v_now);
    END IF;

    -- Item 13: Gà chiên sốt cay ngọt
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Gà chiên sốt cay ngọt';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_side, 'Gà chiên sốt cay ngọt', 'Gà chiên giòn tan rưới sốt Yangnyeom cay ngọt mặn bùng nổ vị giác.', 149000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/ga-chien-sot-cay-ngot.jpg', true, false, false, 35, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 149000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/ga-chien-sot-cay-ngot.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_spicy) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_spicy, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_panchan) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_panchan, 4, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 12000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 15000, v_now, v_now);
    END IF;

    -- Item 14: Khoai tây chiên rong biển
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Khoai tây chiên rong biển';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_side, 'Khoai tây chiên rong biển', 'Khoai tây chiên vàng giòn lắc bột rong biển thơm nức ngậy bùi.', 59000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/khoai-tay-chien-rong-bien.jpg', true, false, false, 50, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 59000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/khoai-tay-chien-rong-bien.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_panchan) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_panchan, 4, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 6000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 8000, v_now, v_now);
    END IF;

    -- Item 15: Salad rong biển mè
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Salad rong biển mè';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_side, 'Salad rong biển mè', 'Salad rong biển tươi mát trộn sốt mè rang béo ngậy chua ngọt thanh vị.', 69000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/salad-rong-bien-me.jpg', true, false, false, 45, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 69000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/salad-rong-bien-me.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_panchan) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_panchan, 4, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 6000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 8000, v_now, v_now);
    END IF;

    -- Item 16: Mandu chiên
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Mandu chiên';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_side, 'Mandu chiên', 'Bánh xếp Hàn Quốc nhân thịt và hẹ chiên giòn rụm chấm sốt xì dầu ớt.', 79000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/mandu-chien.jpg', true, false, false, 40, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 79000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/mandu-chien.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_spicy) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_spicy, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_panchan) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_panchan, 4, 2, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 8000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 10000, v_now, v_now);
    END IF;

    -- Item 17: Soju đào
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Soju đào';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_drink, 'Soju đào', 'Rượu Soju hương đào trái cây ngọt thơm, nồng độ nhẹ thanh mát dễ uống.', 120000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/soju-dao.jpg', true, false, false, 80, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 120000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/soju-dao.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_soju_size) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_soju_size, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 10000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 12000, v_now, v_now);
    END IF;

    -- Item 18: Soju nho xanh
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Soju nho xanh';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_drink, 'Soju nho xanh', 'Rượu Soju hương nho xanh tươi mát cực sảng khoái.', 120000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/soju-nho-xanh.jpg', true, false, false, 80, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 120000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/soju-nho-xanh.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_soju_size) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_soju_size, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 10000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 12000, v_now, v_now);
    END IF;

    -- Item 19: Trà đào cam sả
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Trà đào cam sả';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_drink, 'Trà đào cam sả', 'Trà đào thơm ngát kết hợp vị chua ngọt của cam tươi và hương sả nồng nàn.', 45000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/tra-dao-cam-sa.jpg', true, false, false, 100, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 45000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/tra-dao-cam-sa.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_drink_ice) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_drink_ice, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 5000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 7000, v_now, v_now);
    END IF;

    -- Item 20: Nước gạo Hàn Quốc
    SELECT "MenuItemId" INTO v_item_id FROM "MenuItems" WHERE "Name" = 'Nước gạo Hàn Quốc';
    IF v_item_id IS NULL THEN
        INSERT INTO "MenuItems" ("CategoryId", "Name", "Description", "BasePrice", "ImageUrl", "IsAvailable", "IsDeleted", "IsOutOfStock", "Stock", "CreatedAt", "UpdatedAt")
        VALUES (v_cat_drink, 'Nước gạo Hàn Quốc', 'Nước gạo rang Sikhye ngọt thanh mát lành bổ dưỡng đậm chất truyền thống.', 35000, 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/nuoc-gao-han-quoc.jpg', true, false, false, 90, v_now, v_now)
        RETURNING "MenuItemId" INTO v_item_id;
    ELSE
        UPDATE "MenuItems" SET "BasePrice" = 35000, "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260726/nuoc-gao-han-quoc.jpg', "UpdatedAt" = v_now WHERE "MenuItemId" = v_item_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChoiceGroups" WHERE "MenuItemId" = v_item_id AND "ChoiceGroupId" = v_group_drink_ice) THEN
        INSERT INTO "MenuItemChoiceGroups" ("MenuItemId", "ChoiceGroupId", "MaxSelect", "DisplayOrder", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_group_drink_ice, 1, 1, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_dinein) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_dinein, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_web) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_web, 0, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_shopee) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_shopee, 4000, v_now, v_now);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MenuItemChannelPrices" WHERE "MenuItemId" = v_item_id AND "SalesChannelId" = v_chan_grab) THEN
        INSERT INTO "MenuItemChannelPrices" ("MenuItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_item_id, v_chan_grab, 6000, v_now, v_now);
    END IF;

    ----------------------------------------------------------------------------
    -- 5. CHOICE ITEM CHANNEL PRICES SEEDING
    ----------------------------------------------------------------------------
    FOR v_choice_id IN SELECT "ChoiceItemId" FROM "ChoiceItems" LOOP
        IF NOT EXISTS (SELECT 1 FROM "ChoiceItemChannelPrices" WHERE "ChoiceItemId" = v_choice_id AND "SalesChannelId" = v_chan_dinein) THEN
            INSERT INTO "ChoiceItemChannelPrices" ("ChoiceItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_choice_id, v_chan_dinein, 0, v_now, v_now);
        END IF;
        IF NOT EXISTS (SELECT 1 FROM "ChoiceItemChannelPrices" WHERE "ChoiceItemId" = v_choice_id AND "SalesChannelId" = v_chan_web) THEN
            INSERT INTO "ChoiceItemChannelPrices" ("ChoiceItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_choice_id, v_chan_web, 0, v_now, v_now);
        END IF;
        IF NOT EXISTS (SELECT 1 FROM "ChoiceItemChannelPrices" WHERE "ChoiceItemId" = v_choice_id AND "SalesChannelId" = v_chan_shopee) THEN
            INSERT INTO "ChoiceItemChannelPrices" ("ChoiceItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_choice_id, v_chan_shopee, 2000, v_now, v_now);
        END IF;
        IF NOT EXISTS (SELECT 1 FROM "ChoiceItemChannelPrices" WHERE "ChoiceItemId" = v_choice_id AND "SalesChannelId" = v_chan_grab) THEN
            INSERT INTO "ChoiceItemChannelPrices" ("ChoiceItemId", "SalesChannelId", "ChannelExtraPrice", "CreatedAt", "UpdatedAt") VALUES (v_choice_id, v_chan_grab, 3000, v_now, v_now);
        END IF;
    END LOOP;

    
    ----------------------------------------------------------------------------
    -- 6. UPDATE IMAGES FOR ORIGINAL 15 MENU ITEMS
    ----------------------------------------------------------------------------
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/samgyeopsal.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Ba chỉ heo nướng Samgyeopsal' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/galbi.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Sườn bò nướng Galbi' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/nac-vai-heo-sot-cay.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Nạc vai heo sốt cay' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/budae-jjigae.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Lẩu quân đội Budae-jjigae' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/kimchi-jjigae.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Canh Kimchi Kimchijigae' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/sundubu-jjigae.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Canh đậu hũ non Sundubu-jigae' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/bibimbap.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Cơm trộn Bibimbap' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/jajangmyeon.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Mì tương đen Jajangmyeon' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/tteokbokki.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Bánh gạo cay Tteokbokki' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/japchae.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Miến trộn Japchae' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/kimchi-pajeon.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Bánh xèo Kimchi Pajeon' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/chamisul-soju.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Rượu Soju truyền thống Chamisul' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/makgeolli.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Rượu gạo Makgeolli' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/tra-sam-mat-ong.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Trà sâm mật ong Hàn Quốc' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');
    UPDATE "MenuItems" SET "ImageUrl" = 'https://qcnchscanwgqeyyzipgu.supabase.co/storage/v1/object/public/menu-images/menu-items/20260727/nuoc-suoi.jpg', "UpdatedAt" = v_now WHERE "Name" = 'Nước suối' AND ("ImageUrl" IS NULL OR "ImageUrl" = '' OR "ImageUrl" ILIKE '%.webp%');

    RAISE NOTICE 'DineFlow Korean Menu Seeder completed successfully!';
END $$;
