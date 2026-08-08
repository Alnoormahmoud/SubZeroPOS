using System.Collections.Generic;

namespace SubZeroPOS.Core.DTOs
{
    public class CreateOrderDto
    {
        public int OrderTypeId { get; set; }
        public string? CustomerName { get; set; }
        public decimal DeliveryFee { get; set; }
        public string PaymentMethodCode { get; set; } = "Cash"; // "Cash" / "Bankak"
        public int CashierUserId { get; set; }
        public string? Notes { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
