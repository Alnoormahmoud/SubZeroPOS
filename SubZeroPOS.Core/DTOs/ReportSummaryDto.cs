using System;

namespace SubZeroPOS.Core.DTOs
{
    public class ReportSummaryDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public int TotalOrders { get; set; }

        public decimal NetProfit => TotalIncome - TotalExpenses;
    }
}
