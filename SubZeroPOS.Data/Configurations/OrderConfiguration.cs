using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.OrderId);

            builder.Property(o => o.CustomerName)
                .HasMaxLength(150)
                .IsUnicode(true);

            builder.Property(o => o.OrderDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(o => o.StatusCode)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Completed");

            builder.Property(o => o.PaymentMethodCode)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Cash");

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);

            builder.Property(o => o.DeliveryFee)
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);

            builder.Property(o => o.Notes)
                .HasMaxLength(300)
                .IsUnicode(true);

            builder.HasOne(o => o.OrderType)
                .WithMany(ot => ot.Orders)
                .HasForeignKey(o => o.OrderTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.CashierUser)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CashierUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // matches: CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
            builder.HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Orders_OrderDate");
        }
    }
}
