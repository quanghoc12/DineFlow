# Migration Rules

Dự án dùng **Code First + EF Core Migration** làm nguồn chính quản lý schema database.

## Quy tắc bắt buộc

```text
1. Chỉ DB owner/leader được tạo migration.
2. Member không tự ý chạy dotnet ef migrations add.
3. Member làm feature chỉ sửa Entity của feature mình nếu cần.
4. Nếu sửa Entity ảnh hưởng database, phải báo DB owner.
5. DB owner review Entity + AppDbContext rồi tạo migration.
6. Cả nhóm pull migration mới rồi chạy dotnet ef database update.
7. Không xóa migration đã merge vào develop.
```

## Lệnh tạo migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/DineFlow.DataAccessObjects \
  --startup-project src/DineFlow.Api \
  --context AppDbContext \
  --output-dir Migrations
```

## Lệnh update database

```bash
dotnet ef database update \
  --project src/DineFlow.DataAccessObjects \
  --startup-project src/DineFlow.Api \
  --context AppDbContext
```

## SQL script dùng để làm gì?

SQL trong `database/seed` và `database/manual` chỉ dùng cho:

- Seed data mẫu.
- Reset database local.
- Script đặc biệt.

Không dùng SQL script làm nguồn chính tạo schema.
