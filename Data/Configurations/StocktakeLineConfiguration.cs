using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class StocktakeLineConfiguration
    : IEntityTypeConfiguration<StocktakeLine>
{
    public void Configure(
        EntityTypeBuilder<StocktakeLine> builder)
    {
        builder.ToTable("stocktake_line");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.StocktakeId)
            .HasColumnName("stocktake_id")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(x => x.SystemQty)
            .HasColumnName("system_qty")
            .IsRequired();

        builder.Property(x => x.CountedQty)
            .HasColumnName("counted_qty")
            .IsRequired();

        builder.Property(x => x.DifferenceQty)
            .HasColumnName("difference_qty")
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        builder.HasIndex(x => x.StocktakeId);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.LocationId
        });

        builder.HasOne(x => x.Stocktake)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.StocktakeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}