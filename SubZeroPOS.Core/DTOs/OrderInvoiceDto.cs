using System;
using System.Collections.Generic;

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

        // Restaurant info, pulled from RestaurantSettings at invoice-build time
        public string RestaurantName { get; set; } = string.Empty;
        public string? RestaurantPhone { get; set; }
        public string? RestaurantAddress { get; set; }
        public string? FooterPrimary { get; set; }
        public string? FooterSecondary { get; set; }
        public string CurrencySymbol { get; set; } = "ج.س";
    }
}
