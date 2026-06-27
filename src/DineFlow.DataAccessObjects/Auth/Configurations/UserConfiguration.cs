using DineFlow.BusinessObjects.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DineFlow.DataAccessObjects.Auth.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);
        
        builder.Property(u => u.Username)
               .IsRequired()
               .HasMaxLength(50)
               .IsUnicode(false);

        builder.HasIndex(u => u.Username)
               .IsUnique();

        builder.Property(u => u.PasswordHash)
               .IsRequired()
               .HasMaxLength(255)
               .IsUnicode(false);

        builder.Property(u => u.FullName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
               .IsRequired();

        builder.Property(u => u.UpdatedAt)
               .IsRequired();

        // Foreign Key
        builder.HasOne(u => u.Role)
               .WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.Restrict);

        // Seed Data (Password: 123456)
        string defaultHash = BCrypt.Net.BCrypt.HashPassword("123456");
        var now = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        builder.HasData(
            new User { UserId = 1, Username = "admin", FullName = "Quản Trị Viên Hệ Thống", PasswordHash = defaultHash, RoleId = 1, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { UserId = 2, Username = "staff01", FullName = "Nhân Viên Nguyễn Văn A", PasswordHash = defaultHash, RoleId = 2, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { UserId = 3, Username = "staff_locked", FullName = "Nhân Viên Bị Khóa", PasswordHash = defaultHash, RoleId = 2, IsActive = false, CreatedAt = now, UpdatedAt = now },
            new User { UserId = 4, Username = "admin_backup", FullName = "Quản Trị Viên Dự Phòng", PasswordHash = defaultHash, RoleId = 1, IsActive = true, CreatedAt = now, UpdatedAt = now }
        );
    }
}
