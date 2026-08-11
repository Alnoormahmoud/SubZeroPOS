namespace SubZeroPOS.Core.Entities
{
    /// <summary>
    /// Single-row settings table (always SettingsId = 1). Holds shop info and
    /// invoice header/footer text so it can be edited from a Settings screen
    /// instead of being hardcoded in the app.
    /// </summary>
    public class RestaurantSettings
    {
        public int SettingsId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? InvoiceFooterPrimary { get; set; }   // e.g. "شكراً لزيارتكم"
        public string? InvoiceFooterSecondary { get; set; } // e.g. return/exchange policy
        public string CurrencyCode { get; set; } = "SDG";
        public string CurrencySymbol { get; set; } = "ج.س"; // default: Sudanese Pound
        public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm"; // used on invoice date/time display
        public bool ShowCashierNameOnInvoice { get; set; } = true;
    }
}
