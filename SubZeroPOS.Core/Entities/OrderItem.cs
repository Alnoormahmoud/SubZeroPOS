namespace SubZeroPOS.Core.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // price snapshot at time of sale

        // LineTotal is a DB-computed column (Quantity * UnitPrice) - mapped read-only in EF config
        public decimal LineTotal { get; private set; }

        public Order Order { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
