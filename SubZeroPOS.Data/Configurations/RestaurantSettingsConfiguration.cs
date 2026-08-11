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

            builder.Property(s => s.CurrencyCode)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("SDG");

            builder.Property(s => s.CurrencySymbol)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(true)
                .HasDefaultValue("ج.س");

            builder.Property(s => s.DateFormat)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("yyyy-MM-dd HH:mm");

            builder.Property(s => s.ShowCashierNameOnInvoice)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }
}
