using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Data.Configurations
{
    public class RestaurantSettingsConfiguration
        : IEntityTypeConfiguration<RestaurantSettings>
    {
        public void Configure(EntityTypeBuilder<RestaurantSettings> builder)
        {
            builder.ToTable("RestaurantSettings");

            builder.HasKey(s => s.SettingsId);

            // ==========================================
            // Restaurant information
            // ==========================================

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


            // ==========================================
            // Invoice footer
            // ==========================================

            builder.Property(s => s.InvoiceFooterPrimary)
                .HasMaxLength(200)
                .IsUnicode(true);

            builder.Property(s => s.InvoiceFooterSecondary)
                .HasMaxLength(300)
                .IsUnicode(true);


            // ==========================================
            // Currency
            // ==========================================

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


            // ==========================================
            // Date format
            // ==========================================

            builder.Property(s => s.DateFormat)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("yyyy-MM-dd HH:mm");


            // ==========================================
            // Invoice display settings
            // ==========================================

            builder.Property(s => s.ShowCashierNameOnInvoice)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.ShowLogoOnInvoice)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.ShowOrderNumberOnInvoice)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.ShowCustomerNameOnInvoice)
                .IsRequired()
                .HasDefaultValue(true);


            // ==========================================
            // Paper settings
            // ==========================================

            builder.Property(s => s.ReceiptPaperWidthMm)
                .IsRequired()
                .HasDefaultValue(80);


            // ==========================================
            // Printing behavior
            // ==========================================

            builder.Property(s => s.AutoPrintReceipt)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.OpenPdfAfterPrinting)
                .IsRequired()
                .HasDefaultValue(false);

            // ==========================================
            // Receipt copies
            // ==========================================
            builder.Property(s => s.ReceiptCopies)
                .IsRequired()
                .HasDefaultValue(1);

            // ==========================================
            // Printer name
            // ==========================================
            builder.Property(s => s.PrinterName)
                .HasMaxLength(100)
                .IsUnicode(true);

            // ==========================================
            // Theme
            // ==========================================
            builder.Property(s => s.Theme)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Light");

            // ==========================================
            // Weekly backup settings
            // ==========================================
            builder.Property(s => s.WeeklyBackupEnabled)
                .IsRequired()
                .HasDefaultValue(false);


        }
    }
}