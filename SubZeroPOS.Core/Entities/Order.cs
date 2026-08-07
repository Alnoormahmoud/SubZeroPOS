using System;
using System.Collections.Generic;

namespace SubZeroPOS.Core.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int OrderTypeId { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string StatusCode { get; set; } = "Completed"; // "Completed" / "Cancelled"
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public int CashierUserId { get; set; }
        public string? Notes { get; set; }

        public OrderType OrderType { get; set; } = null!;
        public User CashierUser { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
