-- Replace each URL with the public URL returned by Supabase Storage.
-- This script is safe to run multiple times because it updates by stable menu item name.

UPDATE "MenuItems"
SET "ImageUrl" = 'https://YOUR_PROJECT.supabase.co/storage/v1/object/public/menu-images/menu-items/samgyeopsal.webp'
WHERE "Name" = 'Ba chỉ heo nướng Samgyeopsal';

UPDATE "MenuItems"
SET "ImageUrl" = 'https://YOUR_PROJECT.supabase.co/storage/v1/object/public/menu-images/menu-items/galbi.webp'
WHERE "Name" = 'Sườn bò nướng Galbi';

UPDATE "MenuItems"
SET "ImageUrl" = 'https://YOUR_PROJECT.supabase.co/storage/v1/object/public/menu-images/menu-items/budae-jjigae.webp'
WHERE "Name" = 'Lẩu quân đội Budae-jjigae';

UPDATE "MenuItems"
SET "ImageUrl" = 'https://YOUR_PROJECT.supabase.co/storage/v1/object/public/menu-images/menu-items/kimchi-jjigae.webp'
WHERE "Name" = 'Canh Kimchi Kimchijigae';

UPDATE "MenuItems"
SET "ImageUrl" = 'https://YOUR_PROJECT.supabase.co/storage/v1/object/public/menu-images/menu-items/bibimbap.webp'
WHERE "Name" = 'Cơm trộn Bibimbap';

UPDATE "MenuItems"
SET "ImageUrl" = 'https://YOUR_PROJECT.supabase.co/storage/v1/object/public/menu-images/menu-items/tteokbokki.webp'
WHERE "Name" = 'Bánh gạo cay Tteokbokki';
