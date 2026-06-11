-- Seed data mẫu. Schema chính được tạo bằng EF Core Migration.
-- Password mẫu admin123/staff123 cần được hash bằng BCrypt trong seed chính thức.

INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedAt)
VALUES
('admin', '$2a$11$REPLACE_WITH_BCRYPT_HASH', N'Administrator', 'Admin', 1, SYSUTCDATETIME()),
('staff', '$2a$11$REPLACE_WITH_BCRYPT_HASH', N'Staff User', 'Staff', 1, SYSUTCDATETIME());
