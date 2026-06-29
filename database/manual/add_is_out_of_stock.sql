ALTER TABLE "MenuItems" ADD COLUMN IF NOT EXISTS "IsOutOfStock" boolean NOT NULL DEFAULT false;

-- Register migration in EF migrations history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260629141324_AddMenuItemIsOutOfStock', '8.0.11')
ON CONFLICT DO NOTHING;
