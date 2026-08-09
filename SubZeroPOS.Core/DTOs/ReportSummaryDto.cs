using System;
using System.Collections.Generic;

namespace SubZeroPOS.Core.DTOs
{
    public class ReportSummaryDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public int TotalOrders { get; set; }
        public List<ExpenseByCategoryDto> ExpensesByCategory { get; set; } = new();

        public decimal NetProfit => TotalIncome - TotalExpenses;
    }

    public class ExpenseByCategoryDto
    {
        public string CategoryNameAr { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
