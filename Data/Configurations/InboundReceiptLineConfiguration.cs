using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class InboundReceiptLineConfiguration
    : IEntityTypeConfiguration<InboundReceiptLine>
{
    public void Configure(
        EntityTypeBuilder<InboundReceiptLine> builder)
    {
        builder.ToTable("inbound_line");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InboundReceiptId)
            .HasColumnName("receipt_id")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(x => x.ExpectedQty)
            .HasColumnName("expected_qty")
            .IsRequired();

        builder.Property(x => x.ReceivedQty)
            .HasColumnName("received_qty")
            .IsRequired();

        builder.Property(x => x.PutawayQty)
        .HasColumnName("putaway_qty")
        .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        builder.HasIndex(x => x.InboundReceiptId);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.LocationId
        });

        builder.HasOne(x => x.InboundReceipt)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.InboundReceiptId)
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