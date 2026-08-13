using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class ReportService : IReportService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public ReportService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ReportSummaryDto> GetSummaryAsync(DateTime from, DateTime to)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var start = from.Date;
            var end = to.Date.AddDays(1); // inclusive of the "to" day

            var orders = await context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.StatusCode == "Completed")
                .ToListAsync();

            var orderIds = orders.Select(o => o.OrderId).ToList();

            // Line items for those orders, with Item + Category loaded so we can
            // group by both name and category in one pass.
            var orderItems = await context.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId))
                .Include(oi => oi.Item)
                    .ThenInclude(i => i.Category)
                .ToListAsync();

            var topSellingItems = orderItems
                .GroupBy(oi => oi.Item.ItemName)
                .Select(g => new TopSellingItemDto
                {
                    ItemName = g.Key,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(8)
                .ToList();

            var salesByCategory = orderItems
                .GroupBy(oi => oi.Item.Category.NameAr)
                .Select(g => new CategorySalesDto
                {
                    CategoryNameAr = g.Key,
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    QuantitySold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            var expenses = await context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(e => e.ExpenseDate >= start && e.ExpenseDate < end)
                .ToListAsync();

            var expensesByCategory = expenses
                .GroupBy(e => e.ExpenseCategory.NameAr)
                .Select(g => new ExpenseByCategoryDto
                {
                    CategoryNameAr = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Daily sales trend - one point per day in the range, filling in
            // zero for days with no orders so the chart bars line up correctly.
            var dailySales = new System.Collections.Generic.List<DailySalesDto>();
            for (var day = start; day < end; day = day.AddDays(1))
            {
                var dayTotal = orders
                    .Where(o => o.OrderDate.Date == day)
                    .Sum(o => o.TotalAmount);
                dailySales.Add(new DailySalesDto { Date = day, Total = dayTotal });
            }

            // Order type breakdown (صالة / توصيل / استلام) - covers delivery rate.
            var ordersWithType = await context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.StatusCode == "Completed")
                .Include(o => o.OrderType)
                .ToListAsync();

            var orderTypeBreakdown = ordersWithType
                .GroupBy(o => o.OrderType.NameAr)
                .Select(g => new OrderTypeBreakdownDto
                {
                    OrderTypeNameAr = g.Key,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            return new ReportSummaryDto
            {
                FromDate = from,
                ToDate = to,
                TotalIncome = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count,
                TotalExpenses = expenses.Sum(e => e.Amount),
                ExpensesByCategory = expensesByCategory,
                TopSellingItems = topSellingItems,
                SalesByCategory = salesByCategory,
                DailySales = dailySales,
                OrderTypeBreakdown = orderTypeBreakdown
            };
        }
    }
}
