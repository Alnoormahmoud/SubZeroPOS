using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class RestaurantSettingsService : IRestaurantSettingsService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public RestaurantSettingsService(
            IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public async Task<RestaurantSettings> GetSettingsAsync()
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var settings =
                await context.RestaurantSettings.FirstOrDefaultAsync();

            if (settings != null)
                return settings;


            // ==========================================
            // Fresh installation defaults
            // ==========================================

            settings = new RestaurantSettings
            {
                RestaurantName = "ساب زيرو",

                InvoiceFooterPrimary =
                    "شكراً لزيارتكم",

                InvoiceFooterSecondary = null,

                CurrencyCode = "SDG",
                CurrencySymbol = "ج.س",

                DateFormat = "yyyy-MM-dd HH:mm",

                ShowCashierNameOnInvoice = true,

                ShowLogoOnInvoice = true,

                ShowOrderNumberOnInvoice = true,

                ShowCustomerNameOnInvoice = true,

                ReceiptPaperWidthMm = 80,

                AutoPrintReceipt = false,

                OpenPdfAfterPrinting = false,

                ReceiptCopies = 1,

                PrinterName = string.Empty,

            };

            context.RestaurantSettings.Add(settings);

            await context.SaveChangesAsync();

            return settings;
        }


        public async Task SaveSettingsAsync(
            RestaurantSettings settings)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var existing =
                await context.RestaurantSettings.FirstOrDefaultAsync();


            // ==========================================
            // First settings record
            // ==========================================

            if (existing is null)
            {
                context.RestaurantSettings.Add(settings);
            }

            // ==========================================
            // Update existing settings
            // ==========================================

            else
            {
                // Restaurant information
                existing.RestaurantName =
                    settings.RestaurantName;

                existing.Phone =
                    settings.Phone;

                existing.Address =
                    settings.Address;


                // Invoice footer
                existing.InvoiceFooterPrimary =
                    settings.InvoiceFooterPrimary;

                existing.InvoiceFooterSecondary =
                    settings.InvoiceFooterSecondary;


                // Currency
                existing.CurrencyCode =
                    settings.CurrencyCode;

                existing.CurrencySymbol =
                    settings.CurrencySymbol;


                // Date
                existing.DateFormat =
                    settings.DateFormat;


                // Invoice display
                existing.ShowCashierNameOnInvoice =
                    settings.ShowCashierNameOnInvoice;

                existing.ShowLogoOnInvoice =
                    settings.ShowLogoOnInvoice;

                existing.ShowOrderNumberOnInvoice =
                    settings.ShowOrderNumberOnInvoice;

                existing.ShowCustomerNameOnInvoice =
                    settings.ShowCustomerNameOnInvoice;


                // Paper
                existing.ReceiptPaperWidthMm =
                    settings.ReceiptPaperWidthMm;


                // Printing behavior
                existing.AutoPrintReceipt =
                    settings.AutoPrintReceipt;

                existing.OpenPdfAfterPrinting =
                    settings.OpenPdfAfterPrinting;

                existing.ReceiptCopies =
                    settings.ReceiptCopies;

                existing.PrinterName = settings.PrinterName;
            }


            await context.SaveChangesAsync();
        }
    }
}