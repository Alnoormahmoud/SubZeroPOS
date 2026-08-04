using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class OrderTypeConfiguration : IEntityTypeConfiguration<OrderType>
    {
        public void Configure(EntityTypeBuilder<OrderType> builder)
        {
            builder.ToTable("OrderTypes");

            builder.HasKey(ot => ot.OrderTypeId);

            builder.Property(ot => ot.TypeCode)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.HasIndex(ot => ot.TypeCode).IsUnique();

            builder.Property(ot => ot.NameAr)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(true);
        }
    }
}
