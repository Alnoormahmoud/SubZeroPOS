using System;
using System.Collections.Generic;
using System.Globalization;

namespace SubZeroPOS.Core.DTOs
{
    public class OrderInvoiceDto
    {
        public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

        public string CashierName { get; set; } = string.Empty;

        public string OrderTypeNameAr { get; set; } = string.Empty;

        public string? CustomerName { get; set; }

        public string? Notes { get; set; }

        public decimal DeliveryFee { get; set; }

        public bool HasDeliveryFee => DeliveryFee > 0;

        public string PaymentMethodNameAr { get; set; } = string.Empty;

        public List<CartItemDto> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }


        // ==========================================
        // Restaurant information
        // ==========================================

        public string RestaurantName { get; set; } = string.Empty;

        public string? RestaurantPhone { get; set; }

        public string? RestaurantAddress { get; set; }


        // ==========================================
        // Invoice footer
        // ==========================================

        public string? FooterPrimary { get; set; }

        public string? FooterSecondary { get; set; }


        // ==========================================
        // Currency
        // ==========================================

        public string CurrencySymbol { get; set; } = "ج.س";


        // ==========================================
        // Date and time
        // ==========================================

        public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm";

        public string OrderDateFormatted
        {
            get
            {
                try
                {
                    return OrderDate.ToString(
                        DateFormat,
                        CultureInfo.CurrentCulture);
                }
                catch
                {
                    return OrderDate.ToString(
                        "yyyy-MM-dd HH:mm");
                }
            }
        }


        // ==========================================
        // Invoice display settings
        // ==========================================

        public bool ShowCashierName { get; set; } = true;

        public bool ShowLogoOnInvoice { get; set; } = true;

        public bool ShowOrderNumberOnInvoice { get; set; } = true;

        public bool ShowCustomerNameOnInvoice { get; set; } = true;


        // ==========================================
        // Receipt paper
        // ==========================================

        public int ReceiptPaperWidthMm { get; set; } = 80;


        // ==========================================
        // Printing behavior
        // ==========================================

        public bool OpenPdfAfterPrinting { get; set; } = false;

        public bool ShouldShowCustomerName =>
    ShowCustomerNameOnInvoice &&
    !string.IsNullOrWhiteSpace(CustomerName);

        public bool AutoPrintReceipt { get; set; }
    }
}
