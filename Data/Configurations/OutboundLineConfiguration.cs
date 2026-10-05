using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class OutboundLineConfiguration
    : IEntityTypeConfiguration<OutboundLine>
{
    public void Configure(
        EntityTypeBuilder<OutboundLine> builder)
    {
        builder.ToTable("outbound_line");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.IssueId)
            .HasColumnName("issue_id")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(x => x.RequestedQty)
            .HasColumnName("requested_qty")
            .IsRequired();

        builder.Property(x => x.PickedQty)
            .HasColumnName("picked_qty")
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        builder.HasIndex(x => x.IssueId);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.LocationId
        });

        builder.HasOne(x => x.OutboundIssue)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.IssueId)
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