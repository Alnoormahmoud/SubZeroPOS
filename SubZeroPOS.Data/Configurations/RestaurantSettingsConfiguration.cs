using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class RestaurantSettingsConfiguration : IEntityTypeConfiguration<RestaurantSettings>
    {
        public void Configure(EntityTypeBuilder<RestaurantSettings> builder)
        {
            builder.ToTable("RestaurantSettings");

            builder.HasKey(s => s.SettingsId);

            builder.Property(s => s.RestaurantName)
                .IsRequired()
                .HasMaxLength(150)
                .IsUnicode(true);

            builder.Property(s => s.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(s => s.Address)
                .HasMaxLength(300)
                .IsUnicode(true);

            builder.Property(s => s.InvoiceFooterPrimary)
                .HasMaxLength(200)
                .IsUnicode(true);

            builder.Property(s => s.InvoiceFooterSecondary)
                .HasMaxLength(300)
                .IsUnicode(true);
        }
    }
}
