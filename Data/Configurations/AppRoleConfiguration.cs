using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

/*
 * AppRoleConfiguration.cs
 * 
 * This class configures the AppRole entity for Entity Framework Core. It specifies the table name, primary key, column mappings, constraints, and seed data for the roles.
 */
public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("app_role");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasData(
            new AppRole
            {
                Id = 1,
                Code = "ADMIN",
                Name = "Quản trị viên",
                IsActive = true
            },
            new AppRole
            {
                Id = 2,
                Code = "WAREHOUSE_MANAGER",
                Name = "Quản lý kho",
                IsActive = true
            },
            new AppRole
            {
                Id = 3,
                Code = "WAREHOUSE_OPERATOR",
                Name = "Nhân viên kho",
                IsActive = true
            },
            new AppRole
            {
                Id = 4,
                Code = "VIEWER",
                Name = "Người xem",
                IsActive = true
            }
        );
    }
}