using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class StockTransferConfiguration
    : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(
        EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("stock_transfer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.TransferNo)
            .HasColumnName("transfer_no")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.TransferNo)
            .IsUnique();


        builder.Property(x => x.SourceWarehouseId)
            .HasColumnName("source_warehouse_id")
            .IsRequired();

        builder.Property(x => x.DestinationWarehouseId)
            .HasColumnName("destination_warehouse_id")
            .IsRequired();


        builder.Property(x => x.TransferDate)
            .HasColumnName("transfer_date")
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

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");


        // Source warehouse
        builder.HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);


        // Destination warehouse
        builder.HasOne(x => x.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(x => x.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);


        // Creator
        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(x => new
        {
            x.SourceWarehouseId,
            x.TransferDate
        });
    }
}