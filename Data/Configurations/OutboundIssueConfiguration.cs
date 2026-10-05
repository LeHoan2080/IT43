using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data.Configurations;

public class OutboundIssueConfiguration
    : IEntityTypeConfiguration<OutboundIssue>
{
    public void Configure(
        EntityTypeBuilder<OutboundIssue> builder)
    {
        builder.ToTable("outbound_issue");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.IssueNo)
            .HasColumnName("issue_no")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.IssueNo)
            .IsUnique();

        builder.Property(x => x.WarehouseId)
            .HasColumnName("warehouse_id")
            .IsRequired();

        builder.Property(x => x.IssueReason)
            .HasColumnName("issue_reason")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.IssueDate)
            .HasColumnName("issue_date")
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

        builder.Property(x => x.CompletedBy)
            .HasColumnName("completed_by");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

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
            x.IssueDate
        });
    }
}