# EF Core Migrations

Thư mục này dành cho migration được sinh bởi EF Core.

Rule nhóm:

- Chỉ DB owner/leader được tạo migration.
- Member khác không tự chạy `dotnet ef migrations add`.
- Nếu đổi Entity ảnh hưởng DB, tạo Pull Request và báo DB owner review.
- Không xóa migration đã merge vào `develop` trừ khi cả nhóm thống nhất reset database.
