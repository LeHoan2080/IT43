using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("product");

        // =========================
        // Primary Key
        // =========================
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        // =========================
        // Product Code
        // =========================
        builder.Property(x => x.ProductCode)
            .HasColumnName("product_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.ProductCode)
            .IsUnique();

        // =========================
        // Barcode
        // =========================
        builder.Property(x => x.Barcode)
            .HasColumnName("barcode")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Barcode)
            .IsUnique();

        // =========================
        // Product Name
        // =========================
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // =========================
        // Product Type
        // =========================
        builder.Property(x => x.ProductType)
            .HasColumnName("product_type")
            .HasMaxLength(30)
            .IsRequired();

        // =========================
        // Unit
        // =========================
        builder.Property(x => x.Unit)
            .HasColumnName("unit")
            .HasMaxLength(30)
            .IsRequired();

        // =========================
        // Minimum Stock
        // =========================
        builder.Property(x => x.MinStock)
            .HasColumnName("min_stock")
            .IsRequired();

        // =========================
        // Book Information
        // =========================

        builder.Property(x => x.ISBN)
            .HasColumnName("isbn")
            .HasMaxLength(30);

        builder.HasIndex(x => x.ISBN)
            .IsUnique()
            .HasFilter("[isbn] IS NOT NULL");

        builder.Property(x => x.Author)
            .HasColumnName("author")
            .HasMaxLength(150);

        // =========================
        // Publisher
        // =========================
        builder.Property(x => x.PublisherId)
            .HasColumnName("publisher_id");

        builder.HasOne(x => x.Publisher)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PublishYear)
            .HasColumnName("publication_year");

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        // =========================
        // Stationery Information
        // =========================

        builder.Property(x => x.Brand)
            .HasColumnName("brand")
            .HasMaxLength(100);

        builder.Property(x => x.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.Property(x => x.Specification)
            .HasColumnName("specification")
            .HasMaxLength(500);

        // =========================
        // Notes
        // =========================

        builder.Property(x => x.Notes)
            .HasColumnName("note")
            .HasMaxLength(1000);

        // =========================
        // Status
        // =========================

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // =========================
        // Audit
        // =========================

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");
    }
}