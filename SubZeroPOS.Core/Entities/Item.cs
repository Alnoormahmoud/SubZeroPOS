using System;
using System.Collections.Generic;

namespace SubZeroPOS.Core.Entities
{
    public class Item
    {
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Category Category { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
