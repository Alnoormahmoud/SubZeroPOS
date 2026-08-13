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
        public List<TopSellingItemDto> TopSellingItems { get; set; } = new();
        public List<CategorySalesDto> SalesByCategory { get; set; } = new();
        public List<DailySalesDto> DailySales { get; set; } = new();
        public List<OrderTypeBreakdownDto> OrderTypeBreakdown { get; set; } = new();

        public decimal NetProfit => TotalIncome - TotalExpenses;
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderTypeBreakdownDto
    {
        public string OrderTypeNameAr { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ExpenseByCategoryDto
    {
        public string CategoryNameAr { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class TopSellingItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CategorySalesDto
    {
        public string CategoryNameAr { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int QuantitySold { get; set; }
    }
}
