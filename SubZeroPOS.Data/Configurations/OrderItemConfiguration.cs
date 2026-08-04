using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(oi => oi.OrderItemId);

            builder.Property(oi => oi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(oi => oi.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            // LineTotal is a SQL Server computed column:
            // LineTotal AS (Quantity * UnitPrice) PERSISTED
            // EF must never try to INSERT/UPDATE this - it's calculated by SQL Server.
            builder.Property(oi => oi.LineTotal)
                .HasColumnType("decimal(10,2)")
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", stored: true)
                .ValueGeneratedOnAddOrUpdate();

            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // deleting an order removes its line items

            builder.HasOne(oi => oi.Item)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // matches: CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
            builder.HasIndex(oi => oi.OrderId)
                .HasDatabaseName("IX_OrderItems_OrderId");
        }
    }
}
