using System.Collections.Generic;

namespace SubZeroPOS.Core.Entities
{
    public class OrderType
    {
        public int OrderTypeId { get; set; }
        public string TypeCode { get; set; } = string.Empty; // "DineIn" / "Delivery" / "takeaway"
        public string NameAr { get; set; } = string.Empty;  

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
