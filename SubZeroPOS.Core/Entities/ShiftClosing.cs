using System;

namespace SubZeroPOS.Core.Entities
{
    public class ShiftClosing
    {
        public int ShiftId { get; set; }
        public int UserId { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal OpeningCash { get; set; }
        public decimal? ExpectedCash { get; set; }
        public decimal? ActualCash { get; set; }
        public string? Notes { get; set; }

        public User User { get; set; } = null!;
    }
}
