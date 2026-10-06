using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class StocktakeConfiguration
    : IEntityTypeConfiguration<Stocktake>
{
    public void Configure(
        EntityTypeBuilder<Stocktake> builder)
    {
        builder.ToTable("stocktake");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.StocktakeNo)
            .HasColumnName("stocktake_no")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.StocktakeNo)
            .IsUnique();

        builder.Property(x => x.WarehouseId)
            .HasColumnName("warehouse_id")
            .IsRequired();

        builder.Property(x => x.StocktakeDate)
            .HasColumnName("stocktake_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.CompletedBy)
            .HasColumnName("completed_by");

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Completer)
            .WithMany()
            .HasForeignKey(x => x.CompletedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.WarehouseId,
            x.StocktakeDate
        });
    }
}