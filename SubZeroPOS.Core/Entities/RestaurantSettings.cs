namespace SubZeroPOS.Core.Entities
{
    /// <summary>
    /// Single-row settings table that stores restaurant and invoice settings.
    /// </summary>
    public class RestaurantSettings
    {
        public int SettingsId { get; set; }

        // Restaurant information
        public string RestaurantName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Invoice footer
        public string? InvoiceFooterPrimary { get; set; }
        public string? InvoiceFooterSecondary { get; set; }

        // Currency
        public string CurrencyCode { get; set; } = "SDG";
        public string CurrencySymbol { get; set; } = "ج.س";

        // Date and time
        public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm";

        // Existing invoice option
        public bool ShowCashierNameOnInvoice { get; set; } = true;

        // New invoice settings
        public bool ShowLogoOnInvoice { get; set; } = true;

        public bool ShowOrderNumberOnInvoice { get; set; } = true;

        public bool ShowCustomerNameOnInvoice { get; set; } = true;

        // Receipt paper width
        public int ReceiptPaperWidthMm { get; set; } = 80;

        // Printing behaviorAA
        public bool AutoPrintReceipt { get; set; } = false;

        public bool OpenPdfAfterPrinting { get; set; } = false;
    }
}