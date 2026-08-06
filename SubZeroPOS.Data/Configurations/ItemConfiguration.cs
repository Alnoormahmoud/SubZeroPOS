using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Items");

            builder.HasKey(i => i.ItemId);

            builder.Property(i => i.NameAr)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(true);

            builder.Property(i => i.NameEn)
                .HasMaxLength(200)
                .IsUnicode(false);

            builder.Property(i => i.Price)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(i => i.IsActive)
                .HasDefaultValue(true);

            builder.Property(i => i.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
