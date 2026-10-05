using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class StockTransferLineConfiguration
    : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(
        EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToTable("stock_transfer_line");

        builder.HasKey(x => x.Id);


        builder.Property(x => x.Id)
            .HasColumnName("id");


        builder.Property(x => x.TransferId)
            .HasColumnName("transfer_id")
            .IsRequired();


        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();


        builder.Property(x => x.SourceLocationId)
            .HasColumnName("source_location_id")
            .IsRequired();


        builder.Property(x => x.DestinationLocationId)
            .HasColumnName("destination_location_id")
            .IsRequired();


        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();


        // Transfer -> Lines
        builder.HasOne(x => x.StockTransfer)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.TransferId)
            .OnDelete(DeleteBehavior.Cascade);


        // Product
        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);


        // Source Location
        builder.HasOne(x => x.SourceLocation)
            .WithMany()
            .HasForeignKey(x => x.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);


        // Destination Location
        builder.HasOne(x => x.DestinationLocation)
            .WithMany()
            .HasForeignKey(x => x.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(x => x.TransferId);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.SourceLocationId
        });
    }
}