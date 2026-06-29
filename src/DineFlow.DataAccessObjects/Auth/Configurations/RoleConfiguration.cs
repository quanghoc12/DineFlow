using DineFlow.BusinessObjects.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DineFlow.DataAccessObjects.Auth.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", t => t.HasCheckConstraint("CHK_RoleName_Valid", "RoleName IN ('Admin', 'Staff')"));

        builder.HasKey(r => r.RoleId);

        builder.Property(r => r.RoleName)
               .IsRequired()
               .HasMaxLength(50)
               .IsUnicode(false);

        builder.HasIndex(r => r.RoleName)
               .IsUnique();

        // Seed Data
        builder.HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Staff" }
        );
    }
}
